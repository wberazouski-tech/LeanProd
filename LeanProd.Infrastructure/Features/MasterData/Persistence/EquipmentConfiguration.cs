using LeanProd.Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.MasterData.Persistence;

internal sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.InventoryNumber).HasMaxLength(50);
        builder.Property(x => x.SerialNumber).HasMaxLength(100);
        builder.Property(x => x.Manufacturer).HasMaxLength(200);
        builder.Property(x => x.Model).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.InventoryNumber).IsUnique().HasFilter("[InventoryNumber] IS NOT NULL");
        builder.HasIndex(x => x.Name); builder.HasIndex(x => x.IsActive);
        builder.HasOne(x => x.EquipmentType).WithMany(x => x.Equipment)
            .HasForeignKey(x => x.EquipmentTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Department).WithMany(x => x.Equipment)
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ParentEquipment).WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentEquipmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class EquipmentTypeConfiguration : IEntityTypeConfiguration<EquipmentType>
{
    public void Configure(EntityTypeBuilder<EquipmentType> builder)
    {
        builder.ToTable("EquipmentTypes"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Name).IsUnique(); builder.HasIndex(x => x.IsActive);
    }
}

internal sealed class EquipmentStateEventConfiguration : IEntityTypeConfiguration<EquipmentStateEvent>
{
    public void Configure(EntityTypeBuilder<EquipmentStateEvent> builder)
    {
        builder.ToTable("EquipmentStateEvents", table => table.HasCheckConstraint(
            "CK_EquipmentStateEvents_Period", "[EndedAtUtc] IS NULL OR [EndedAtUtc] > [StartedAtUtc]"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.State).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(1000);
        builder.HasIndex(x => new { x.EquipmentId, x.StartedAtUtc }).IsUnique();
        builder.HasIndex(x => x.EquipmentId).IsUnique().HasFilter("[EndedAtUtc] IS NULL");
        builder.HasOne(x => x.Equipment).WithMany(x => x.StateEvents)
            .HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
