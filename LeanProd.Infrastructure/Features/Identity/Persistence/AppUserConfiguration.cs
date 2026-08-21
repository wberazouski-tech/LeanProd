using LeanProd.Application.Features.Identity;
using LeanProd.Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.Identity.Persistence;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(x => x.PreferredLanguage)
            .HasMaxLength(10)
            .HasDefaultValue(SupportedLanguages.Belarusian)
            .IsRequired();
        builder.HasOne<Department>().WithMany().HasForeignKey(x => x.DefaultDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StorageLocation>().WithMany().HasForeignKey(x => x.DefaultStorageLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
