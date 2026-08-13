using LeanProd.Application.Common.Abstractions;
using LeanProd.Domain.Common;
using LeanProd.Infrastructure.Features.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Common.Persistence;

public sealed class LeanProdDbContext(
    DbContextOptions<LeanProdDbContext> options,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(LeanProdDbContext).Assembly);

        foreach (var entityType in builder.Model.GetEntityTypes()
                     .Where(type => typeof(AuditableEntity).IsAssignableFrom(type.ClrType)))
        {
            builder.Entity(entityType.ClrType)
                .Property(nameof(AuditableEntity.RowVersion))
                .IsRowVersion();
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditValues();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditValues();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditValues()
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var userId = currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.CreatedByUserId = userId;
                entry.Entity.UpdatedAtUtc = null;
                entry.Entity.UpdatedByUserId = null;
                continue;
            }

            if (entry.State != EntityState.Modified) continue;

            entry.Property(entity => entity.CreatedAtUtc).IsModified = false;
            entry.Property(entity => entity.CreatedByUserId).IsModified = false;
            entry.Entity.UpdatedAtUtc = now;
            entry.Entity.UpdatedByUserId = userId;
        }
    }
}
