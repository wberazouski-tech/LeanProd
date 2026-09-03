using LeanProd.Domain.Common;
using LeanProd.Domain.MasterData;

namespace LeanProd.Domain.Workforce;

public sealed class Brigade : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public bool IsActive { get; set; } = true;
    public List<BrigadeMembership> Memberships { get; set; } = [];
}
