using LeanProd.Application.Features.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.Identity.Persistence;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PreferredLanguage)
            .HasMaxLength(10)
            .HasDefaultValue(SupportedLanguages.Belarusian)
            .IsRequired();
        builder.HasIndex(x => x.DefaultDepartmentId);
        builder.HasIndex(x => x.DefaultStorageLocationId);
    }
}
