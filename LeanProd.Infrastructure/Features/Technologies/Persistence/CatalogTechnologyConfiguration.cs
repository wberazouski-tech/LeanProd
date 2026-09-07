using LeanProd.Domain.MasterData;
using LeanProd.Domain.Technologies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.Technologies.Persistence;

internal sealed class CatalogTechnologyConfiguration : IEntityTypeConfiguration<CatalogTechnology>
{
    public void Configure(EntityTypeBuilder<CatalogTechnology> builder)
    {
        builder.ToTable("CatalogTechnologies", table =>
        {
            table.HasCheckConstraint("CK_CatalogTechnologies_Target",
                "([CatalogItemId] IS NOT NULL AND [CatalogItemClassId] IS NULL) OR ([CatalogItemId] IS NULL AND [CatalogItemClassId] IS NOT NULL)");
            table.HasCheckConstraint("CK_CatalogTechnologies_ValidPeriod",
                "[ValidTo] IS NULL OR [ValidFrom] IS NULL OR [ValidTo] >= [ValidFrom]");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired()
            .HasDefaultValue(CatalogTechnologyStatus.InDevelopment);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.IsActive, x.Name });
        builder.HasIndex(x => new { x.CatalogItemId, x.IsDefault })
            .IsUnique()
            .HasFilter("[CatalogItemId] IS NOT NULL AND [IsDefault] = 1 AND [IsActive] = 1");
        builder.HasIndex(x => new { x.CatalogItemClassId, x.IsDefault })
            .IsUnique()
            .HasFilter("[CatalogItemClassId] IS NOT NULL AND [IsDefault] = 1 AND [IsActive] = 1");
        builder.HasOne(x => x.CatalogItem).WithMany()
            .HasForeignKey(x => x.CatalogItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CatalogItemClass).WithMany()
            .HasForeignKey(x => x.CatalogItemClassId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class TechnologyStageTemplateConfiguration : IEntityTypeConfiguration<TechnologyStageTemplate>
{
    public void Configure(EntityTypeBuilder<TechnologyStageTemplate> builder)
    {
        builder.ToTable("TechnologyStageTemplates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.IsActive, x.Name });
    }
}

internal sealed class CatalogTechnologyStageConfiguration : IEntityTypeConfiguration<CatalogTechnologyStage>
{
    public void Configure(EntityTypeBuilder<CatalogTechnologyStage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PlannedDurationMinutes).HasPrecision(18, 3);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.ToTable("CatalogTechnologyStages", table =>
            table.HasCheckConstraint("CK_CatalogTechnologyStages_StageNumber", "[StageNumber] > 0"));
        builder.HasIndex(x => new { x.CatalogTechnologyId, x.StageNumber }).IsUnique();
        builder.HasIndex(x => new { x.Id, x.CatalogTechnologyId }).IsUnique();
        builder.HasOne(x => x.CatalogTechnology).WithMany(x => x.Stages)
            .HasForeignKey(x => x.CatalogTechnologyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.TechnologyStage).WithMany(x => x.Usages)
            .HasForeignKey(x => x.TechnologyStageId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Equipment).WithMany()
            .HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class TechnologyStageConfiguration : IEntityTypeConfiguration<TechnologyStage>
{
    public void Configure(EntityTypeBuilder<TechnologyStage> builder)
    {
        builder.ToTable("TechnologyStages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.Name, x.DepartmentId });
        builder.HasOne(x => x.Department).WithMany()
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.IsActive, x.Name });
    }
}

internal sealed class CatalogTechnologyStageTransitionConfiguration : IEntityTypeConfiguration<CatalogTechnologyStageTransition>
{
    public void Configure(EntityTypeBuilder<CatalogTechnologyStageTransition> builder)
    {
        builder.ToTable("CatalogTechnologyStageTransitions", table =>
            table.HasCheckConstraint("CK_CatalogTechnologyStageTransitions_NoSelfTransition", "[FromCatalogTechnologyStageId] <> [ToCatalogTechnologyStageId]"));
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.CatalogTechnologyId, x.FromCatalogTechnologyStageId, x.ToCatalogTechnologyStageId }).IsUnique();
        builder.HasOne(x => x.CatalogTechnology).WithMany(x => x.StageTransitions)
            .HasForeignKey(x => x.CatalogTechnologyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.FromCatalogTechnologyStage).WithMany()
            .HasForeignKey(x => new { x.FromCatalogTechnologyStageId, x.CatalogTechnologyId })
            .HasPrincipalKey(x => new { x.Id, x.CatalogTechnologyId }).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.ToCatalogTechnologyStage).WithMany()
            .HasForeignKey(x => new { x.ToCatalogTechnologyStageId, x.CatalogTechnologyId })
            .HasPrincipalKey(x => new { x.Id, x.CatalogTechnologyId }).OnDelete(DeleteBehavior.NoAction);
    }
}

internal sealed class CatalogTechnologyMaterialConfiguration : IEntityTypeConfiguration<CatalogTechnologyMaterial>
{
    public void Configure(EntityTypeBuilder<CatalogTechnologyMaterial> builder)
    {
        builder.ToTable("CatalogTechnologyMaterials", table =>
        {
            table.HasCheckConstraint("CK_CatalogTechnologyMaterials_Quantity", "[Quantity] > 0");
            table.HasCheckConstraint("CK_CatalogTechnologyMaterials_ScrapPercent", "[ScrapPercent] >= 0 AND [ScrapPercent] <= 100");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.ScrapPercent).HasPrecision(9, 4);
        builder.Property(x => x.ConsumptionTrackingMode).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.HasIndex(x => x.TechnologyStageId);
        builder.HasOne(x => x.TechnologyStage).WithMany(x => x.Materials)
            .HasForeignKey(x => x.TechnologyStageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.CatalogItem).WithMany()
            .HasForeignKey(x => x.CatalogItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UnitOfMeasure).WithMany()
            .HasForeignKey(x => x.UnitOfMeasureId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DefaultSourceStorageLocation).WithMany()
            .HasForeignKey(x => x.DefaultSourceStorageLocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CatalogTechnologyStageOutputConfiguration : IEntityTypeConfiguration<CatalogTechnologyStageOutput>
{
    public void Configure(EntityTypeBuilder<CatalogTechnologyStageOutput> builder)
    {
        builder.ToTable("CatalogTechnologyStageOutputs", table =>
            table.HasCheckConstraint("CK_CatalogTechnologyStageOutputs_Quantity", "[Quantity] > 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.HasIndex(x => x.TechnologyStageId);
        builder.HasOne(x => x.TechnologyStage).WithMany(x => x.Outputs)
            .HasForeignKey(x => x.TechnologyStageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.CatalogItem).WithMany()
            .HasForeignKey(x => x.CatalogItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UnitOfMeasure).WithMany()
            .HasForeignKey(x => x.UnitOfMeasureId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ReceiptStorageLocation).WithMany()
            .HasForeignKey(x => x.ReceiptStorageLocationId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CatalogTechnologyOperationConfiguration : IEntityTypeConfiguration<CatalogTechnologyOperation>
{
    public void Configure(EntityTypeBuilder<CatalogTechnologyOperation> builder)
    {
        builder.ToTable("CatalogTechnologyOperations", table =>
        {
            table.HasCheckConstraint("CK_CatalogTechnologyOperations_Minutes", "[SetupMinutes] >= 0 AND [RunMinutes] >= 0 AND [LaborMinutes] >= 0");
            table.HasCheckConstraint("CK_CatalogTechnologyOperations_Workers", "[Workers] > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SetupMinutes).HasPrecision(18, 3);
        builder.Property(x => x.RunMinutes).HasPrecision(18, 3);
        builder.Property(x => x.LaborMinutes).HasPrecision(18, 3);
        builder.Property(x => x.Workers).HasPrecision(18, 3);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TechnologyStageId, x.Code }).IsUnique();
        builder.HasOne(x => x.TechnologyStage).WithMany(x => x.Operations)
            .HasForeignKey(x => x.TechnologyStageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Department).WithMany()
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Equipment).WithMany()
            .HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CatalogTechnologyMaterialSupplyRouteStepConfiguration
    : IEntityTypeConfiguration<CatalogTechnologyMaterialSupplyRouteStep>
{
    public void Configure(EntityTypeBuilder<CatalogTechnologyMaterialSupplyRouteStep> builder)
    {
        builder.ToTable("CatalogTechnologyMaterialSupplyRouteSteps", table =>
        {
            table.HasCheckConstraint("CK_CatalogTechnologyMaterialSupplyRouteSteps_Target",
                "([ToStorageLocationId] IS NOT NULL AND [IsConsumptionPoint] = 0) OR ([ToStorageLocationId] IS NULL AND [IsConsumptionPoint] = 1)");
            table.HasCheckConstraint("CK_CatalogTechnologyMaterialSupplyRouteSteps_LeadTime", "[LeadTimeMinutes] >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MovementKind).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.LeadTimeMinutes).HasPrecision(18, 3);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TechnologyMaterialId, x.LineNo }).IsUnique();
        builder.HasOne(x => x.TechnologyMaterial).WithMany(x => x.RouteSteps)
            .HasForeignKey(x => x.TechnologyMaterialId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.FromStorageLocation).WithMany()
            .HasForeignKey(x => x.FromStorageLocationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ToStorageLocation).WithMany()
            .HasForeignKey(x => x.ToStorageLocationId).OnDelete(DeleteBehavior.Restrict);
    }
}
