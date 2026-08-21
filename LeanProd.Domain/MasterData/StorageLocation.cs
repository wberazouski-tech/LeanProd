using LeanProd.Domain.Common;
using LeanProd.Domain.Organizations;

namespace LeanProd.Domain.MasterData;

public sealed class StorageLocation : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public Guid KindId { get; set; }
    public StorageLocationKind Kind { get; set; } = null!;
    public Guid? ParentStorageLocationId { get; set; }
    public StorageLocation? ParentStorageLocation { get; set; }
    public List<StorageLocation> Children { get; set; } = [];
    public List<StorageLocationTypeAssignment> TypeAssignments { get; set; } = [];
    public List<Address> Addresses { get; set; } = [];
    public bool IsActive { get; set; } = true;
}
