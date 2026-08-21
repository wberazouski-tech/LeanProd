namespace LeanProd.Infrastructure.Features.Identity;

public sealed class SecurityAuditEvent
{
    public long Id { get; set; }
    public Guid ActorUserId { get; set; }
    public Guid TargetUserId { get; set; }
    public required string Action { get; set; }
    public required string TraceId { get; set; }
    public string? Details { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}
