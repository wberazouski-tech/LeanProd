using LeanProd.Domain.Common;
using LeanProd.Domain.MasterData;

namespace LeanProd.Domain.Workforce;

public sealed class Employee : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PersonnelNumber { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Position { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public bool IsActive { get; set; } = true;
    public List<BrigadeMembership> BrigadeMemberships { get; set; } = [];
}
