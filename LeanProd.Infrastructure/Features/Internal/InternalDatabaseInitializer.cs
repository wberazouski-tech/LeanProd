using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanProd.Infrastructure.Features.Internal;

public static class InternalDatabaseInitializer
{
    public static async Task InitializeInternalDatabaseAsync(
        this IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.InternalOrganizations.AnyAsync(cancellationToken)) return;

        var section = configuration.GetSection("InternalOrganization");
        var now = DateTime.UtcNow;
        db.InternalOrganizations.Add(new InternalOrganization
        {
            Id = InternalOrganization.SingletonId,
            InstallationId = Guid.NewGuid(),
            LegalName = section["LegalName"]?.Trim() ?? "LeanProd installation",
            TradingName = Clean(section["TradingName"]),
            LegalForm = Clean(section["LegalForm"]),
            CountryCode = section["CountryCode"]?.Trim().ToUpperInvariant() ?? "BY",
            TaxNumber = Clean(section["TaxNumber"]),
            CompanyRegistrationNumber = Clean(section["CompanyRegistrationNumber"]),
            DefaultCurrencyCode = section["DefaultCurrencyCode"]?.Trim().ToUpperInvariant() ?? "BYN",
            TimeZoneId = section["TimeZoneId"]?.Trim() ?? "Europe/Minsk",
            DefaultLanguageCode = section["DefaultLanguageCode"]?.Trim().ToLowerInvariant() ?? "be",
            IsConfigured = !string.IsNullOrWhiteSpace(section["LegalName"]),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
