using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.MasterData;
using LeanProd.Application.Features.Organizations;
using LeanProd.Domain.Organizations;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.Organizations;

public sealed class OrganizationService(LeanProdDbContext db) : IOrganizationService
{
    public async Task<OrganizationDetails?> GetAsync(CancellationToken ct)
    {
        var item = await db.Organizations.AsNoTracking().SingleOrDefaultAsync(x => x.Id == Organization.SingletonId, ct);
        return item is null ? null : Details(item);
    }

    public async Task<MasterDataResult<OrganizationDetails>> SaveAsync(SaveOrganizationCommand c, CancellationToken ct)
    {
        var error = Validate(c); if (error is not null) return Failure(error);
        var item = await db.Organizations.SingleOrDefaultAsync(x => x.Id == Organization.SingletonId, ct);
        if (item is null) { item = new Organization(); db.Organizations.Add(item); }
        else if (!SetVersion(item, c.RowVersion)) return Failure("Row version is required.");
        item.LegalName = c.LegalName.Trim(); item.TradingName = Clean(c.TradingName); item.LegalForm = Clean(c.LegalForm);
        item.CountryCode = c.CountryCode.Trim().ToUpperInvariant(); item.TaxNumber = Clean(c.TaxNumber);
        item.StatisticalNumber = Clean(c.StatisticalNumber); item.CompanyRegistrationNumber = Clean(c.CompanyRegistrationNumber);
        item.DefaultCurrencyCode = c.DefaultCurrencyCode.Trim().ToUpperInvariant(); item.TimeZoneId = c.TimeZoneId.Trim();
        item.DefaultLanguageCode = c.DefaultLanguageCode.Trim().ToLowerInvariant(); item.Email = Clean(c.Email);
        item.Phone = Clean(c.Phone); item.Website = Clean(c.Website); item.LogoFileId = c.LogoFileId; item.PrintFooter = Clean(c.PrintFooter);
        try { await db.SaveChangesAsync(ct); return MasterDataResult<OrganizationDetails>.Success(Details(item)); }
        catch (DbUpdateConcurrencyException) { return MasterDataResult<OrganizationDetails>.Failure(MasterDataError.Conflict, "Organization data was changed by another request."); }
    }
    private static string? Validate(SaveOrganizationCommand c)
    {
        if (string.IsNullOrWhiteSpace(c.LegalName)) return "Legal name is required.";
        if (c.CountryCode.Trim().Length != 2) return "Country code must contain two letters.";
        if (c.DefaultCurrencyCode.Trim().Length != 3) return "Currency code must contain three letters.";
        if (string.IsNullOrWhiteSpace(c.TimeZoneId)) return "Time zone is required.";
        if (!SupportedLanguages.IsSupported(c.DefaultLanguageCode)) return "The language is not supported.";
        return null;
    }
    private bool SetVersion(Organization item, string? value) { try { if (string.IsNullOrWhiteSpace(value)) return false; db.Entry(item).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(value); return true; } catch (FormatException) { return false; } }
    private static OrganizationDetails Details(Organization x) => new(x.Id, x.LegalName, x.TradingName, x.LegalForm,
        x.CountryCode, x.TaxNumber, x.StatisticalNumber, x.CompanyRegistrationNumber, x.DefaultCurrencyCode,
        x.TimeZoneId, x.DefaultLanguageCode, x.Email, x.Phone, x.Website, x.LogoFileId, x.PrintFooter,
        Convert.ToBase64String(x.RowVersion));
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static MasterDataResult<OrganizationDetails> Failure(string message) => MasterDataResult<OrganizationDetails>.Failure(MasterDataError.Validation, message);
}
