namespace LeanProd.Infrastructure.Features.Internal;

public sealed class InternalOrganization
{
    public const byte SingletonId = 1;
    public byte Id { get; set; } = SingletonId;
    public Guid InstallationId { get; set; } = Guid.NewGuid();
    public string LegalName { get; set; } = "LeanProd installation";
    public string? TradingName { get; set; }
    public string? LegalForm { get; set; }
    public string CountryCode { get; set; } = "BY";
    public string? TaxNumber { get; set; }
    public string? CompanyRegistrationNumber { get; set; }
    public string DefaultCurrencyCode { get; set; } = "BYN";
    public string TimeZoneId { get; set; } = "Europe/Minsk";
    public string DefaultLanguageCode { get; set; } = "be";
    public bool IsConfigured { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
