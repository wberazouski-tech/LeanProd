using LeanProd.Domain.Common;

namespace LeanProd.Domain.MasterData;

public sealed class EquipmentType : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Equipment> Equipment { get; set; } = [];
}
