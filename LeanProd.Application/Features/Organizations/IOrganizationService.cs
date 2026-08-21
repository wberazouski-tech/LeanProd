using LeanProd.Application.Features.MasterData;

namespace LeanProd.Application.Features.Organizations;

public interface IOrganizationService
{
    Task<OrganizationDetails?> GetAsync(CancellationToken ct);
    Task<MasterDataResult<OrganizationDetails>> SaveAsync(SaveOrganizationCommand command, CancellationToken ct);
}

public sealed record OrganizationDetails(byte Id, string LegalName, string? TradingName, string? LegalForm,
    string CountryCode, string? TaxNumber, string? StatisticalNumber, string? CompanyRegistrationNumber,
    string DefaultCurrencyCode, string TimeZoneId, string DefaultLanguageCode, string? Email, string? Phone,
    string? Website, Guid? LogoFileId, string? PrintFooter, string RowVersion);
public sealed record SaveOrganizationCommand(string LegalName, string? TradingName, string? LegalForm,
    string CountryCode, string? TaxNumber, string? StatisticalNumber, string? CompanyRegistrationNumber,
    string DefaultCurrencyCode, string TimeZoneId, string DefaultLanguageCode, string? Email, string? Phone,
    string? Website, Guid? LogoFileId, string? PrintFooter, string? RowVersion);
