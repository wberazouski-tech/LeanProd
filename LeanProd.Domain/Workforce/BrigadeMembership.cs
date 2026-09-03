using LeanProd.Domain.Common;

namespace LeanProd.Domain.Workforce;

public sealed class BrigadeMembership : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BrigadeId { get; set; }
    public Brigade Brigade { get; set; } = null!;
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public decimal LaborParticipationCoefficient { get; set; } = 1m;
}
