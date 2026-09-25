using LeanProd.Domain.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.MasterData.Persistence;

internal sealed class ItemPropertyDefinitionConfiguration : IEntityTypeConfiguration<ItemPropertyDefinition>
{
    public void Configure(EntityTypeBuilder<ItemPropertyDefinition> b)
    {
        b.ToTable("ItemPropertyDefinitions", t => t.HasCheckConstraint("CK_ItemPropertyDefinitions_Settings", "([Type] = 'Number' AND [DecimalPlaces] BETWEEN 0 AND 6 AND [DecimalPlaces] IS NOT NULL AND [MaxLength] IS NULL AND [Minimum] IS NULL AND [Maximum] IS NULL) OR ([Type] = 'Range' AND [DecimalPlaces] BETWEEN 0 AND 6 AND [DecimalPlaces] IS NOT NULL AND [MaxLength] IS NULL AND [Minimum] IS NOT NULL AND [Maximum] IS NOT NULL AND [Minimum] <= [Maximum]) OR ([Type] = 'Text' AND [MaxLength] BETWEEN 1 AND 4000 AND [MaxLength] IS NOT NULL AND [DecimalPlaces] IS NULL AND [Minimum] IS NULL AND [Maximum] IS NULL) OR ([Type] IN ('Choice', 'Boolean') AND [MaxLength] IS NULL AND [DecimalPlaces] IS NULL AND [Minimum] IS NULL AND [Maximum] IS NULL)"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Minimum).HasPrecision(24, 6);
        b.Property(x => x.Maximum).HasPrecision(24, 6);
        b.HasIndex(x => x.CatalogItemClassId);
        b.HasOne<CatalogItemClass>().WithMany().HasForeignKey(x => x.CatalogItemClassId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Options).WithOne().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ItemPropertyOptionConfiguration : IEntityTypeConfiguration<ItemPropertyOption>
{
    public void Configure(EntityTypeBuilder<ItemPropertyOption> b)
    {
        b.ToTable("ItemPropertyOptions");
        b.HasKey(x => x.Id);
        b.HasAlternateKey(x => new { x.PropertyId, x.Id });
        b.Property(x => x.Label).HasMaxLength(200).IsRequired();
        b.HasIndex(x => new { x.PropertyId, x.Label }).IsUnique();
    }
}

internal sealed class CatalogItemPropertyValueConfiguration : IEntityTypeConfiguration<CatalogItemPropertyValue>
{
    public void Configure(EntityTypeBuilder<CatalogItemPropertyValue> b)
    {
        PropertyValueConfiguration.Configure(b, "CatalogItemPropertyValues");
        b.HasKey(x => new { x.CatalogItemId, x.PropertyId });
        b.HasOne<CatalogItem>().WithMany().HasForeignKey(x => x.CatalogItemId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class BatchPropertyValueConfiguration : IEntityTypeConfiguration<BatchPropertyValue>
{
    public void Configure(EntityTypeBuilder<BatchPropertyValue> b)
    {
        PropertyValueConfiguration.Configure(b, "BatchPropertyValues");
        b.HasKey(x => new { x.BatchId, x.PropertyId });
        b.HasOne<CatalogItemBatch>().WithMany(x => x.Values).HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal static class PropertyValueConfiguration
{
    public static void Configure<T>(EntityTypeBuilder<T> b, string table) where T : class
    {
        b.ToTable(table, t => t.HasCheckConstraint($"CK_{table}_Shape", "([Number] IS NOT NULL AND [Text] IS NULL AND [Boolean] IS NULL AND [OptionId] IS NULL AND ([Upper] IS NULL OR [Number] <= [Upper])) OR ([Text] IS NOT NULL AND [Number] IS NULL AND [Upper] IS NULL AND [Boolean] IS NULL AND [OptionId] IS NULL) OR ([Boolean] IS NOT NULL AND [Number] IS NULL AND [Upper] IS NULL AND [Text] IS NULL AND [OptionId] IS NULL) OR ([OptionId] IS NOT NULL AND [Number] IS NULL AND [Upper] IS NULL AND [Text] IS NULL AND [Boolean] IS NULL)"));
        b.Property<decimal?>("Number").HasPrecision(24, 6);
        b.Property<decimal?>("Upper").HasPrecision(24, 6);
        b.Property<string?>("Text").HasMaxLength(4000);
        b.HasOne<ItemPropertyDefinition>().WithMany().HasForeignKey("PropertyId").OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ItemPropertyOption>().WithMany().HasForeignKey("PropertyId", "OptionId")
            .HasPrincipalKey(x => new { x.PropertyId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CatalogItemBatchConfiguration : IEntityTypeConfiguration<CatalogItemBatch>
{
    public void Configure(EntityTypeBuilder<CatalogItemBatch> b)
    {
        b.ToTable("CatalogItemBatches");
        b.HasKey(x => x.Id);
        b.Property(x => x.Number).HasMaxLength(100).IsRequired();
        b.Property(x => x.ReceiptReference).HasMaxLength(300).IsRequired();
        b.HasIndex(x => new { x.CatalogItemId, x.Number }).IsUnique();
        b.HasOne<CatalogItem>().WithMany().HasForeignKey(x => x.CatalogItemId).OnDelete(DeleteBehavior.Restrict);
    }
}
