using LeanProd.Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.MasterData.Persistence;

internal sealed class StorageLocationKindConfiguration : IEntityTypeConfiguration<StorageLocationKind>
{
    public void Configure(EntityTypeBuilder<StorageLocationKind> builder)
    {
        builder.ToTable("StorageLocationKinds"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired(); builder.HasIndex(x => x.Code).IsUnique();
        builder.HasData(StorageLocationCatalogCodes.Kinds.Select((code, index) => new StorageLocationKind
        { Id = Guid.Parse($"10000000-0000-0000-0000-{index + 1:000000000000}"), Code = code, IsActive = true }));
    }
}

internal sealed class StorageLocationTypeConfiguration : IEntityTypeConfiguration<StorageLocationType>
{
    public void Configure(EntityTypeBuilder<StorageLocationType> builder)
    {
        builder.ToTable("StorageLocationTypes"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired(); builder.HasIndex(x => x.Code).IsUnique();
        builder.HasData(StorageLocationCatalogCodes.Types.Select((code, index) => new StorageLocationType
        { Id = Guid.Parse($"20000000-0000-0000-0000-{index + 1:000000000000}"), Code = code, IsActive = true }));
    }
}

internal sealed class StorageLocationTypeAssignmentConfiguration : IEntityTypeConfiguration<StorageLocationTypeAssignment>
{
    public void Configure(EntityTypeBuilder<StorageLocationTypeAssignment> builder)
    {
        builder.ToTable("StorageLocationTypeAssignments");
        builder.HasKey(x => new { x.StorageLocationId, x.StorageLocationTypeId });
        builder.HasOne(x => x.StorageLocation).WithMany(x => x.TypeAssignments)
            .HasForeignKey(x => x.StorageLocationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.StorageLocationType).WithMany(x => x.Assignments)
            .HasForeignKey(x => x.StorageLocationTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}
