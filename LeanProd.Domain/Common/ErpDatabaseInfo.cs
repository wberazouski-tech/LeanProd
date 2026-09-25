namespace LeanProd.Domain.Common;

public sealed class ErpDatabaseInfo
{
    public const byte SingletonId = 1;
    public byte Id { get; set; } = SingletonId;
    public Guid DatabaseId { get; set; } = Guid.NewGuid();
    public Guid InstallationId { get; set; }
    public string OrganizationLegalName { get; set; } = string.Empty;
    public string? OrganizationTaxNumber { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string CreatedByApplicationVersion { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
