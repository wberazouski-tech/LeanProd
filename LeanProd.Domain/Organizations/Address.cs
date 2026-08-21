using LeanProd.Domain.Common;
using LeanProd.Domain.MasterData;

namespace LeanProd.Domain.Organizations;

public sealed class Address : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public byte? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid? StorageLocationId { get; set; }
    public StorageLocation? StorageLocation { get; set; }
    public string AddressType { get; set; } = AddressTypeCodes.Other;
    public string CountryCode { get; set; } = string.Empty;
    public string? Locality { get; set; }
    public string? PostalCode { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string? Gln { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
}

public static class AddressTypeCodes
{
    public const string Registered = "Registered";
    public const string Correspondence = "Correspondence";
    public const string Office = "Office";
    public const string Delivery = "Delivery";
    public const string Other = "Other";
    public static readonly IReadOnlyCollection<string> All = [Registered, Correspondence, Office, Delivery, Other];
}
