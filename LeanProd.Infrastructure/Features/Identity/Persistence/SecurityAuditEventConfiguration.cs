using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.Identity.Persistence;

internal sealed class SecurityAuditEventConfiguration : IEntityTypeConfiguration<SecurityAuditEvent>
{
    public void Configure(EntityTypeBuilder<SecurityAuditEvent> builder)
    {
        builder.ToTable("SecurityAuditEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TraceId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Details).HasMaxLength(2000);
        builder.HasIndex(x => x.OccurredAtUtc);
        builder.HasIndex(x => x.TargetUserId);
        builder.HasIndex(x => x.ActorUserId);
    }
}
