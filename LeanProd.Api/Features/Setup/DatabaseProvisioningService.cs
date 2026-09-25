using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Application.Common.Abstractions;
using LeanProd.Domain.Common;
using LeanProd.Infrastructure.Common.Persistence;
using LeanProd.Infrastructure.Features.Internal;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LeanProd.Api.Features.Setup;

public interface IDatabaseProvisioningService
{
    DatabaseSetupStatus GetStatus();
    Task TestAdminConnectionAsync(DatabaseAdminConnectionRequest request, CancellationToken cancellationToken);
    Task<DatabaseSetupResult> ConnectExistingAsync(
        ConnectExistingDatabaseRequest request,
        CancellationToken cancellationToken);
    Task<DatabaseSetupResult> CreateNewAsync(CreateDatabaseRequest request, CancellationToken cancellationToken);
}

public sealed class DatabaseProvisioningService(
    IDatabaseSettingsStore store,
    IConfiguration configuration,
    DatabaseStartupState startupState,
    IdentityDbContext identityDbContext) : IDatabaseProvisioningService
{
    public DatabaseSetupStatus GetStatus()
    {
        var persisted = store.Load();
        if (persisted is not null)
        {
            return new DatabaseSetupStatus(
                true,
                startupState.DatabaseInitializationFailed,
                persisted.Server,
                persisted.Database,
                persisted.AppLogin);
        }

        var configured = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            var builder = new SqlConnectionStringBuilder(configured);
            return new DatabaseSetupStatus(
                true,
                startupState.DatabaseInitializationFailed,
                builder.DataSource,
                builder.InitialCatalog,
                string.IsNullOrWhiteSpace(builder.UserID) ? null : builder.UserID);
        }

        return new DatabaseSetupStatus(false, true, null, null, null);
    }

    public async Task TestAdminConnectionAsync(DatabaseAdminConnectionRequest request, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(BuildAdminConnectionString(request, "master"));
        await connection.OpenAsync(cancellationToken);
        _ = await DatabaseExistsAsync(connection, request.Database, cancellationToken);
    }

    public async Task<DatabaseSetupResult> ConnectExistingAsync(
        ConnectExistingDatabaseRequest request,
        CancellationToken cancellationToken)
    {
        await using var masterConnection = new SqlConnection(BuildAdminConnectionString(request.Connection, "master"));
        await masterConnection.OpenAsync(cancellationToken);
        if (!await DatabaseExistsAsync(masterConnection, request.Connection.Database, cancellationToken))
            throw new InvalidOperationException("Selected database does not exist.");

        var adminConnectionString = BuildAdminConnectionString(request.Connection, request.Connection.Database);
        if (!await IsLeanProdDatabaseAsync(adminConnectionString, cancellationToken))
        {
            return new DatabaseSetupResult(
                DatabaseSetupStatuses.NotLeanProd,
                "Selected database is not a LeanProd database.",
                request.Connection.Server,
                request.Connection.Database,
                request.Connection.AppLogin,
                [],
                []);
        }

        var version = await ReadMigrationStateAsync(adminConnectionString, cancellationToken);
        if (version.HasUnknownMigrations)
            throw new InvalidOperationException("Database schema is newer than this application version.");
        if (version.Pending.Count > 0 && request.ApplyMigrations)
            await RunMigrationsAsync(adminConnectionString, cancellationToken);

        var appConnectionString = await ProvisionAppUserAsync(masterConnection, request.Connection, cancellationToken);
        await VerifyAppConnectionAsync(appConnectionString, cancellationToken);
        store.Save("SqlServer", appConnectionString, request.Connection.Server, request.Connection.Database,
            request.Connection.AppLogin);
        startupState.DatabaseInitializationFailed = false;

        var updatedVersion = await ReadMigrationStateAsync(adminConnectionString, cancellationToken);
        return new DatabaseSetupResult(
            DatabaseSetupStatuses.Connected,
            version.Pending.Count > 0 && request.ApplyMigrations
                ? "Database migrated and connected."
                : version.Pending.Count > 0
                    ? "Database connected; schema update is available."
                    : "Database connected.",
            request.Connection.Server,
            request.Connection.Database,
            request.Connection.AppLogin,
            updatedVersion.Applied,
            updatedVersion.Pending);
    }

    public async Task<DatabaseSetupResult> CreateNewAsync(
        CreateDatabaseRequest request,
        CancellationToken cancellationToken)
    {
        await using var masterConnection = new SqlConnection(BuildAdminConnectionString(request.Connection, "master"));
        await masterConnection.OpenAsync(cancellationToken);
        if (await DatabaseExistsAsync(masterConnection, request.Connection.Database, cancellationToken))
            throw new InvalidOperationException("Database already exists.");

        await ExecuteNonQueryAsync(masterConnection,
            $"CREATE DATABASE {QuoteName(request.Connection.Database)};",
            cancellationToken);

        var adminConnectionString = BuildAdminConnectionString(request.Connection, request.Connection.Database);
        await RunMigrationsAsync(adminConnectionString, cancellationToken);
        await WriteDatabaseInfoAsync(adminConnectionString, cancellationToken);

        var appConnectionString = await ProvisionAppUserAsync(masterConnection, request.Connection, cancellationToken);
        await VerifyAppConnectionAsync(appConnectionString, cancellationToken);
        store.Save("SqlServer", appConnectionString, request.Connection.Server, request.Connection.Database,
            request.Connection.AppLogin);
        startupState.DatabaseInitializationFailed = false;

        var version = await ReadMigrationStateAsync(adminConnectionString, cancellationToken);
        return new DatabaseSetupResult(
            DatabaseSetupStatuses.Created,
            "Database created and connected.",
            request.Connection.Server,
            request.Connection.Database,
            request.Connection.AppLogin,
            version.Applied,
            version.Pending);
    }

    private static async Task<bool> DatabaseExistsAsync(
        SqlConnection connection,
        string databaseName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM sys.databases WHERE name = @name;";
        command.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 128) { Value = databaseName });
        var value = (int)await command.ExecuteScalarAsync(cancellationToken);
        return value > 0;
    }

    private static async Task<bool> IsLeanProdDatabaseAsync(string adminConnectionString, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(adminConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT CASE WHEN
                OBJECT_ID(N'ErpDatabaseInfo', N'U') IS NOT NULL
            THEN 1 ELSE 0 END;
            """;
        var value = (int)await command.ExecuteScalarAsync(cancellationToken);
        return value == 1;
    }

    private static async Task<MigrationState> ReadMigrationStateAsync(
        string adminConnectionString,
        CancellationToken cancellationToken)
    {
        await using var dbContext = CreateDbContext(adminConnectionString);
        var known = dbContext.Database.GetMigrations().ToList();
        var applied = (await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken)).ToList();
        var pending = known.Except(applied, StringComparer.OrdinalIgnoreCase).ToList();
        var unknown = applied.Except(known, StringComparer.OrdinalIgnoreCase)
            .Any(id => !DatabaseSchemaVersions.LegacyBusinessMigrationIds.Contains(id));
        return new MigrationState(applied, pending, unknown);
    }

    private static async Task RunMigrationsAsync(string adminConnectionString, CancellationToken cancellationToken)
    {
        await using var dbContext = CreateDbContext(adminConnectionString);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }


    private async Task WriteDatabaseInfoAsync(
        string adminConnectionString,
        CancellationToken cancellationToken)
    {
        await using var dbContext = CreateDbContext(adminConnectionString);
        if (await dbContext.ErpDatabaseInfo.AnyAsync(cancellationToken)) return;

        var organization = await identityDbContext.InternalOrganizations.AsNoTracking()
            .SingleAsync(x => x.Id == InternalOrganization.SingletonId, cancellationToken);
        var version = typeof(DatabaseProvisioningService).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? typeof(DatabaseProvisioningService).Assembly.GetName().Version?.ToString()
            ?? "unknown";
        dbContext.ErpDatabaseInfo.Add(new ErpDatabaseInfo
        {
            Id = ErpDatabaseInfo.SingletonId,
            DatabaseId = Guid.NewGuid(),
            InstallationId = organization.InstallationId,
            OrganizationLegalName = organization.LegalName,
            OrganizationTaxNumber = organization.TaxNumber,
            CountryCode = organization.CountryCode,
            CreatedByApplicationVersion = version,
            SchemaVersion = DatabaseSchemaVersions.BusinessBaselineMigrationId,
            CreatedAtUtc = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    private static LeanProdDbContext CreateDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<LeanProdDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(LeanProdDbContext).Assembly.FullName))
            .Options;
        return new LeanProdDbContext(options, new SetupCurrentUser(), TimeProvider.System);
    }

    private static async Task<string> ProvisionAppUserAsync(
        SqlConnection masterConnection,
        DatabaseAdminConnectionRequest request,
        CancellationToken cancellationToken)
    {
        var appPassword = GeneratePassword();
        var loginName = NormalizeSqlName(request.AppLogin, nameof(request.AppLogin));
        if (loginName.Equals("sa", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The application SQL login cannot be 'sa'. Use a dedicated login, for example 'leanprod_app'.");
        if (loginName.Equals(request.AdminLogin.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The application SQL login must be different from the SQL administrator login.");
        var userName = loginName;

        await ExecuteNonQueryAsync(masterConnection,
            $"""
            IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = N'{SqlLiteral(loginName)}')
                EXEC(N'CREATE LOGIN {QuoteName(loginName)} WITH PASSWORD = N''{SqlLiteral(appPassword)}'', CHECK_POLICY = ON, CHECK_EXPIRATION = OFF');
            ELSE
                EXEC(N'ALTER LOGIN {QuoteName(loginName)} WITH PASSWORD = N''{SqlLiteral(appPassword)}'', CHECK_POLICY = ON, CHECK_EXPIRATION = OFF');
            """,
            cancellationToken);

        await using var databaseConnection = new SqlConnection(BuildAdminConnectionString(request, request.Database));
        await databaseConnection.OpenAsync(cancellationToken);
        await ExecuteNonQueryAsync(databaseConnection,
            $"""
            IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'{SqlLiteral(userName)}')
                CREATE USER {QuoteName(userName)} FOR LOGIN {QuoteName(loginName)};
            IF ISNULL(IS_ROLEMEMBER(N'db_datareader', N'{SqlLiteral(userName)}'), 0) <> 1
                ALTER ROLE db_datareader ADD MEMBER {QuoteName(userName)};
            IF ISNULL(IS_ROLEMEMBER(N'db_datawriter', N'{SqlLiteral(userName)}'), 0) <> 1
                ALTER ROLE db_datawriter ADD MEMBER {QuoteName(userName)};
            """,
            cancellationToken);

        return BuildAppConnectionString(request, appPassword, loginName);
    }

    private static async Task VerifyAppConnectionAsync(string connectionString, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1;";
        await command.ExecuteScalarAsync(cancellationToken);
    }

    private static async Task ExecuteNonQueryAsync(
        SqlConnection connection,
        string commandText,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.CommandTimeout = 120;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string BuildAdminConnectionString(DatabaseAdminConnectionRequest request, string database)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = request.Server,
            InitialCatalog = database,
            UserID = request.AdminLogin,
            Password = request.AdminPassword,
            Encrypt = request.Encrypt,
            TrustServerCertificate = request.TrustServerCertificate,
            MultipleActiveResultSets = true
        };
        return builder.ConnectionString;
    }

    private static string BuildAppConnectionString(
        DatabaseAdminConnectionRequest request,
        string appPassword,
        string appLogin)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = request.Server,
            InitialCatalog = request.Database,
            UserID = appLogin,
            Password = appPassword,
            Encrypt = request.Encrypt,
            TrustServerCertificate = request.TrustServerCertificate,
            MultipleActiveResultSets = true
        };
        return builder.ConnectionString;
    }

    private static string GeneratePassword() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) + "aA1!";

    private static string NormalizeSqlName(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SQL name is required.", parameterName);
        if (value.Length > 128)
            throw new ArgumentException("SQL name cannot exceed 128 characters.", parameterName);
        return value.Trim();
    }

    private static string QuoteName(string value) => "[" + value.Replace("]", "]]") + "]";
    private static string SqlLiteral(string value) => value.Replace("'", "''");

    private sealed record MigrationState(
        IReadOnlyList<string> Applied,
        IReadOnlyList<string> Pending,
        bool HasUnknownMigrations);

    private sealed class SetupCurrentUser : ICurrentUser
    {
        public Guid? UserId => null;
    }
}
