using LeanProd.Domain.Common;

namespace LeanProd.Domain.MasterData;

public sealed class EquipmentStateEvent : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;
    public string State { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public string? Comment { get; set; }
}

public static class EquipmentStateCodes
{
    public const string Operational = "Operational";
    public const string Maintenance = "Maintenance";
    public const string Repair = "Repair";
    public const string OutOfService = "OutOfService";
    public const string Decommissioned = "Decommissioned";
    public static readonly IReadOnlyCollection<string> All =
        [Operational, Maintenance, Repair, OutOfService, Decommissioned];
}
