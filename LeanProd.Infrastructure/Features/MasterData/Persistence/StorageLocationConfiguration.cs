using LeanProd.Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.MasterData.Persistence;

internal sealed class StorageLocationConfiguration : IEntityTypeConfiguration<StorageLocation>
{
    public void Configure(EntityTypeBuilder<StorageLocation> builder)
    {
        builder.ToTable("StorageLocations"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(4).IsFixedLength().IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Code).IsUnique(); builder.HasIndex(x => x.IsActive);
        builder.HasOne(x => x.Department).WithMany(x => x.StorageLocations)
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Kind).WithMany(x => x.StorageLocations)
            .HasForeignKey(x => x.KindId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ParentStorageLocation).WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentStorageLocationId).OnDelete(DeleteBehavior.Restrict);
    }
}
