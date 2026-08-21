using LeanProd.Domain.Common;
using LeanProd.Domain.Organizations;

namespace LeanProd.Domain.MasterData;

public sealed class Department : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }
    public List<Department> Children { get; set; } = [];
    public List<StorageLocation> StorageLocations { get; set; } = [];
    public List<Equipment> Equipment { get; set; } = [];
    public List<Address> Addresses { get; set; } = [];
    public bool IsActive { get; set; } = true;
}
