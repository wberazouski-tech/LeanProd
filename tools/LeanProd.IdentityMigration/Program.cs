using LeanProd.Domain.Common;
using LeanProd.Infrastructure.Common.Persistence;
using LeanProd.Infrastructure.Features.Identity;
using LeanProd.Infrastructure.Features.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using InternalIdentityDbContext = LeanProd.Infrastructure.Common.Persistence.IdentityDbContext;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.IdentityMigration;

internal static class Program
{
    private const string SourceVariable = "LEANPROD_LEGACY_SQL_CONNECTION";
    private const string DestinationVariable = "LEANPROD_INTERNAL_SQLITE_PATH";

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var sourceConnection = RequiredEnvironmentVariable(SourceVariable);
            var destinationPath = Path.GetFullPath(RequiredEnvironmentVariable(DestinationVariable));
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)
                ?? throw new InvalidOperationException("The SQLite path must include a directory."));

            var sourceOptions = new DbContextOptionsBuilder<LegacyIdentityExportDbContext>()
                .UseSqlServer(sourceConnection)
                .Options;
            var destinationOptions = new DbContextOptionsBuilder<InternalIdentityDbContext>()
                .UseSqlite($"Data Source={destinationPath}", sqlite =>
                    sqlite.MigrationsAssembly(typeof(InternalIdentityDbContext).Assembly.FullName))
                .Options;

            await using var source = new LegacyIdentityExportDbContext(sourceOptions);
            await using var destination = new InternalIdentityDbContext(destinationOptions);
            if (!await source.Database.CanConnectAsync())
                throw new InvalidOperationException("Cannot connect to the legacy SQL Server database.");

            await destination.Database.MigrateAsync();
            if (args.Contains("--adopt-business", StringComparer.OrdinalIgnoreCase))
            {
                await AdoptBusinessBaselineAsync(sourceConnection, destination);
                Console.WriteLine("Existing ERP database adopted by the business baseline; legacy Identity tables were not deleted.");
                return 0;
            }
            if (await destination.Users.AnyAsync() || await destination.Roles.AnyAsync() ||
                await destination.RefreshTokens.AnyAsync() || await destination.SecurityAuditEvents.AnyAsync())
                throw new InvalidOperationException(
                    "The MySQL Identity store is not empty. Export is intentionally non-destructive and cannot overwrite it.");

            await using var transaction = await destination.Database.BeginTransactionAsync();
            await CopyIdentityAsync(source, destination);
            await CopyOrganizationAsync(sourceConnection, destination);
            await transaction.CommitAsync();

            Console.WriteLine("Identity export completed successfully.");
            Console.WriteLine($"Users: {await destination.Users.CountAsync()}; roles: {await destination.Roles.CountAsync()}; refresh tokens: {await destination.RefreshTokens.CountAsync()}.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Identity export failed: {exception.Message}");
            return 1;
        }
    }

    private static async Task CopyIdentityAsync(
        LegacyIdentityExportDbContext source,
        InternalIdentityDbContext destination)
    {
        var roles = await source.Roles.AsNoTracking().ToArrayAsync();
        destination.Roles.AddRange(roles.Select(role => new IdentityRole<Guid>
        {
            Id = role.Id,
            Name = role.Name,
            NormalizedName = role.NormalizedName,
            ConcurrencyStamp = role.ConcurrencyStamp
        }));

        var users = await source.Users.AsNoTracking().ToArrayAsync();
        destination.Users.AddRange(users.Select(user => new AppUser
        {
            Id = user.Id,
            UserName = user.UserName,
            NormalizedUserName = user.NormalizedUserName,
            Email = user.Email,
            NormalizedEmail = user.NormalizedEmail,
            EmailConfirmed = user.EmailConfirmed,
            PasswordHash = user.PasswordHash,
            SecurityStamp = user.SecurityStamp,
            ConcurrencyStamp = user.ConcurrencyStamp,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LockoutEnd = user.LockoutEnd,
            LockoutEnabled = user.LockoutEnabled,
            AccessFailedCount = user.AccessFailedCount,
            DisplayName = user.DisplayName,
            IsActive = user.IsActive,
            PreferredLanguage = user.PreferredLanguage,
            DefaultDepartmentId = user.DefaultDepartmentId,
            DefaultStorageLocationId = user.DefaultStorageLocationId,
            CreatedAtUtc = user.CreatedAtUtc,
            CreatedByUserId = user.CreatedByUserId,
            UpdatedAtUtc = user.UpdatedAtUtc,
            UpdatedByUserId = user.UpdatedByUserId,
            LastLoginAtUtc = user.LastLoginAtUtc
        }));
        await destination.SaveChangesAsync();

        destination.UserRoles.AddRange((await source.UserRoles.AsNoTracking().ToArrayAsync())
            .Select(item => new IdentityUserRole<Guid> { UserId = item.UserId, RoleId = item.RoleId }));
        destination.UserClaims.AddRange((await source.UserClaims.AsNoTracking().ToArrayAsync())
            .Select(item => new IdentityUserClaim<Guid>
            {
                Id = item.Id, UserId = item.UserId, ClaimType = item.ClaimType, ClaimValue = item.ClaimValue
            }));
        destination.RoleClaims.AddRange((await source.RoleClaims.AsNoTracking().ToArrayAsync())
            .Select(item => new IdentityRoleClaim<Guid>
            {
                Id = item.Id, RoleId = item.RoleId, ClaimType = item.ClaimType, ClaimValue = item.ClaimValue
            }));
        destination.UserLogins.AddRange((await source.UserLogins.AsNoTracking().ToArrayAsync())
            .Select(item => new IdentityUserLogin<Guid>
            {
                LoginProvider = item.LoginProvider,
                ProviderKey = item.ProviderKey,
                ProviderDisplayName = item.ProviderDisplayName,
                UserId = item.UserId
            }));
        destination.UserTokens.AddRange((await source.UserTokens.AsNoTracking().ToArrayAsync())
            .Select(item => new IdentityUserToken<Guid>
            {
                UserId = item.UserId, LoginProvider = item.LoginProvider, Name = item.Name, Value = item.Value
            }));
        destination.RefreshTokens.AddRange((await source.RefreshTokens.AsNoTracking().ToArrayAsync())
            .Select(item => new RefreshToken
            {
                Id = item.Id,
                UserId = item.UserId,
                FamilyId = item.FamilyId,
                TokenHash = item.TokenHash,
                CreatedAtUtc = item.CreatedAtUtc,
                ExpiresAtUtc = item.ExpiresAtUtc,
                RevokedAtUtc = item.RevokedAtUtc,
                ReplacedByTokenHash = item.ReplacedByTokenHash
            }));
        destination.SecurityAuditEvents.AddRange((await source.SecurityAuditEvents.AsNoTracking().ToArrayAsync())
            .Select(item => new SecurityAuditEvent
            {
                Id = item.Id,
                ActorUserId = item.ActorUserId,
                TargetUserId = item.TargetUserId,
                Action = item.Action,
                TraceId = item.TraceId,
                Details = item.Details,
                OccurredAtUtc = item.OccurredAtUtc
            }));
        await destination.SaveChangesAsync();
    }

    private static async Task CopyOrganizationAsync(
        string sourceConnectionString,
        InternalIdentityDbContext destination)
    {
        if (await destination.InternalOrganizations.AnyAsync()) return;

        await using var connection = new SqlConnection(sourceConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            IF OBJECT_ID(N'Organization', N'U') IS NULL
                SELECT CAST(0 AS bit) AS Found, NULL AS LegalName, NULL AS TradingName, NULL AS LegalForm,
                       NULL AS CountryCode, NULL AS TaxNumber, NULL AS CompanyRegistrationNumber,
                       NULL AS DefaultCurrencyCode, NULL AS TimeZoneId, NULL AS DefaultLanguageCode;
            ELSE
                SELECT TOP (1) CAST(1 AS bit) AS Found, LegalName, TradingName, LegalForm, CountryCode,
                       TaxNumber, CompanyRegistrationNumber, DefaultCurrencyCode, TimeZoneId, DefaultLanguageCode
                FROM Organization WHERE Id = 1;
            """;
        await using var reader = await command.ExecuteReaderAsync();
        var found = await reader.ReadAsync() && reader.GetBoolean(0);
        var now = DateTime.UtcNow;
        destination.InternalOrganizations.Add(new InternalOrganization
        {
            Id = InternalOrganization.SingletonId,
            InstallationId = Guid.NewGuid(),
            LegalName = found ? RequiredString(reader, 1, "LeanProd installation") : "LeanProd installation",
            TradingName = found ? OptionalString(reader, 2) : null,
            LegalForm = found ? OptionalString(reader, 3) : null,
            CountryCode = found ? RequiredString(reader, 4, "BY") : "BY",
            TaxNumber = found ? OptionalString(reader, 5) : null,
            CompanyRegistrationNumber = found ? OptionalString(reader, 6) : null,
            DefaultCurrencyCode = found ? RequiredString(reader, 7, "BYN") : "BYN",
            TimeZoneId = found ? RequiredString(reader, 8, "Europe/Minsk") : "Europe/Minsk",
            DefaultLanguageCode = found ? RequiredString(reader, 9, "be") : "be",
            IsConfigured = found,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
        await destination.SaveChangesAsync();
    }


    private static async Task AdoptBusinessBaselineAsync(
        string sourceConnectionString,
        InternalIdentityDbContext identity)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LEANPROD_BACKUP_CONFIRMED"), "true",
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Create and verify a SQL Server backup, then set LEANPROD_BACKUP_CONFIRMED=true.");

        var organization = await identity.InternalOrganizations.AsNoTracking()
            .SingleAsync(x => x.Id == InternalOrganization.SingletonId);
        await using var connection = new SqlConnection(sourceConnectionString);
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        var applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var history = connection.CreateCommand())
        {
            history.Transaction = transaction;
            history.CommandText = "SELECT MigrationId FROM __EFMigrationsHistory;";
            await using var reader = await history.ExecuteReaderAsync();
            while (await reader.ReadAsync()) applied.Add(reader.GetString(0));
        }
        var missing = DatabaseSchemaVersions.LegacyBusinessMigrationIds.Except(applied).ToArray();
        if (missing.Length > 0)
            throw new InvalidOperationException(
                $"The legacy ERP schema is incomplete; missing migration: {missing[0]}.");

        await using (var schema = connection.CreateCommand())
        {
            schema.Transaction = transaction;
            schema.CommandText = """
                IF OBJECT_ID(N'ErpDatabaseInfo', N'U') IS NULL
                BEGIN
                    CREATE TABLE ErpDatabaseInfo
                    (
                        Id tinyint NOT NULL,
                        DatabaseId uniqueidentifier NOT NULL,
                        InstallationId uniqueidentifier NOT NULL,
                        OrganizationLegalName nvarchar(300) NOT NULL,
                        OrganizationTaxNumber nvarchar(50) NULL,
                        CountryCode nchar(2) NOT NULL,
                        CreatedByApplicationVersion nvarchar(100) NOT NULL,
                        SchemaVersion nvarchar(150) NOT NULL,
                        CreatedAtUtc datetime2 NOT NULL,
                        CONSTRAINT PK_ErpDatabaseInfo PRIMARY KEY (Id),
                        CONSTRAINT CK_ErpDatabaseInfo_Singleton CHECK (Id = 1)
                    );
                    CREATE UNIQUE INDEX IX_ErpDatabaseInfo_DatabaseId ON ErpDatabaseInfo(DatabaseId);
                END;
                """;
            await schema.ExecuteNonQueryAsync();
        }

        await using (var metadata = connection.CreateCommand())
        {
            metadata.Transaction = transaction;
            metadata.CommandText = """
                IF NOT EXISTS (SELECT 1 FROM ErpDatabaseInfo WHERE Id = 1)
                    INSERT ErpDatabaseInfo
                        (Id, DatabaseId, InstallationId, OrganizationLegalName, OrganizationTaxNumber,
                         CountryCode, CreatedByApplicationVersion, SchemaVersion, CreatedAtUtc)
                    VALUES
                        (1, @databaseId, @installationId, @legalName, @taxNumber,
                         @countryCode, @applicationVersion, @schemaVersion, @createdAtUtc);
                IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = @migrationId)
                    INSERT __EFMigrationsHistory (MigrationId, ProductVersion)
                    VALUES (@migrationId, @productVersion);
                """;
            metadata.Parameters.AddWithValue("@databaseId", Guid.NewGuid());
            metadata.Parameters.AddWithValue("@installationId", organization.InstallationId);
            metadata.Parameters.AddWithValue("@legalName", organization.LegalName);
            metadata.Parameters.AddWithValue("@taxNumber", (object?)organization.TaxNumber ?? DBNull.Value);
            metadata.Parameters.AddWithValue("@countryCode", organization.CountryCode);
            metadata.Parameters.AddWithValue("@applicationVersion",
                typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown");
            metadata.Parameters.AddWithValue("@schemaVersion", DatabaseSchemaVersions.BusinessBaselineMigrationId);
            metadata.Parameters.AddWithValue("@createdAtUtc", DateTime.UtcNow);
            metadata.Parameters.AddWithValue("@migrationId", DatabaseSchemaVersions.BusinessBaselineMigrationId);
            metadata.Parameters.AddWithValue("@productVersion", "8.0.13");
            await metadata.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
    private static string RequiredEnvironmentVariable(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Set the {name} environment variable.");

    private static string RequiredString(SqlDataReader reader, int ordinal, string fallback) =>
        reader.IsDBNull(ordinal) ? fallback : reader.GetString(ordinal);

    private static string? OptionalString(SqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
}

internal sealed class LegacyIdentityExportDbContext(
    DbContextOptions<LegacyIdentityExportDbContext> options)
    : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SecurityAuditEvent> SecurityAuditEvents => Set<SecurityAuditEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppUser).Assembly,
            type => type.Namespace?.Contains("Features.Identity.Persistence", StringComparison.Ordinal) == true);
    }
}
