using LeanProd.Domain.Common;

namespace LeanProd.Domain.MasterData;

public sealed class Equipment : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? InventoryNumber { get; set; }
    public Guid? EquipmentTypeId { get; set; }
    public EquipmentType? EquipmentType { get; set; }
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public Guid? ParentEquipmentId { get; set; }
    public Equipment? ParentEquipment { get; set; }
    public List<Equipment> Children { get; set; } = [];
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateOnly? CommissionedOn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<EquipmentStateEvent> StateEvents { get; set; } = [];
}
