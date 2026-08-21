using LeanProd.Domain.Common;

namespace LeanProd.Domain.Organizations;

public sealed class Organization : AuditableEntity
{
    public const byte SingletonId = 1;
    public byte Id { get; set; } = SingletonId;
    public string LegalName { get; set; } = string.Empty;
    public string? TradingName { get; set; }
    public string? LegalForm { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? StatisticalNumber { get; set; }
    public string? CompanyRegistrationNumber { get; set; }
    public string DefaultCurrencyCode { get; set; } = string.Empty;
    public string TimeZoneId { get; set; } = string.Empty;
    public string DefaultLanguageCode { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public Guid? LogoFileId { get; set; }
    public string? PrintFooter { get; set; }
    public List<Address> Addresses { get; set; } = [];
}
