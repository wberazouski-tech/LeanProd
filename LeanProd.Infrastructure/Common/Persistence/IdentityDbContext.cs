using LeanProd.Infrastructure.Features.Identity;
using LeanProd.Infrastructure.Features.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Common.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SecurityAuditEvent> SecurityAuditEvents => Set<SecurityAuditEvent>();
    public DbSet<InternalOrganization> InternalOrganizations => Set<InternalOrganization>();
    public DbSet<ApplicationSetting> ApplicationSettings => Set<ApplicationSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly,
            type => type.Namespace is not null &&
                (type.Namespace.Contains("Features.Identity.Persistence", StringComparison.Ordinal) ||
                 type.Namespace.Contains("Features.Internal.Persistence", StringComparison.Ordinal)));
    }
}
