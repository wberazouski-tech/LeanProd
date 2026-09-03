using LeanProd.Domain.Common;
using LeanProd.Domain.MasterData;

namespace LeanProd.Domain.Technologies;

public sealed class CatalogTechnology : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? CatalogItemId { get; set; }
    public CatalogItem? CatalogItem { get; set; }
    public Guid? CatalogItemClassId { get; set; }
    public CatalogItemClass? CatalogItemClass { get; set; }
    public int VersionNo { get; set; } = 1;
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsDefault { get; set; }
    public CatalogTechnologyStatus Status { get; set; } = CatalogTechnologyStatus.InDevelopment;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<CatalogTechnologyStage> Stages { get; set; } = [];
    public List<CatalogTechnologyStageLink> StageLinks { get; set; } = [];
}

public enum CatalogTechnologyStatus
{
    InDevelopment = 1,
    Active = 2,
    NotUsed = 3
}

public sealed class TechnologyStageTemplate : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<CatalogTechnologyStage> Stages { get; set; } = [];
}

public sealed class CatalogTechnologyStage : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CatalogTechnologyId { get; set; }
    public CatalogTechnology CatalogTechnology { get; set; } = null!;
    public Guid? StageTemplateId { get; set; }
    public TechnologyStageTemplate? StageTemplate { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int LineNo { get; set; }
    public decimal? PlannedDurationMinutes { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid? EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }
    public string? Description { get; set; }
    public List<CatalogTechnologyMaterial> Materials { get; set; } = [];
    public List<CatalogTechnologyStageOutput> Outputs { get; set; } = [];
    public List<CatalogTechnologyOperation> Operations { get; set; } = [];
}

public sealed class CatalogTechnologyStageLink : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CatalogTechnologyId { get; set; }
    public CatalogTechnology CatalogTechnology { get; set; } = null!;
    public Guid FromStageId { get; set; }
    public CatalogTechnologyStage FromStage { get; set; } = null!;
    public Guid ToStageId { get; set; }
    public CatalogTechnologyStage ToStage { get; set; } = null!;
    public TechnologyStageLinkType LinkType { get; set; } = TechnologyStageLinkType.FinishToStart;
    public decimal LagMinutes { get; set; }
}

public sealed class CatalogTechnologyMaterial : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TechnologyStageId { get; set; }
    public CatalogTechnologyStage TechnologyStage { get; set; } = null!;
    public Guid CatalogItemId { get; set; }
    public CatalogItem CatalogItem { get; set; } = null!;
    public Guid UnitOfMeasureId { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
    public decimal Quantity { get; set; }
    public TechnologyMaterialConsumptionTrackingMode ConsumptionTrackingMode { get; set; }
        = TechnologyMaterialConsumptionTrackingMode.NormOnly;
    public Guid? DefaultSourceStorageLocationId { get; set; }
    public StorageLocation? DefaultSourceStorageLocation { get; set; }
    public decimal ScrapPercent { get; set; }
    public bool IsOptional { get; set; }
    public string? Note { get; set; }
    public List<CatalogTechnologyMaterialSupplyRouteStep> RouteSteps { get; set; } = [];
}

public sealed class CatalogTechnologyStageOutput : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TechnologyStageId { get; set; }
    public CatalogTechnologyStage TechnologyStage { get; set; } = null!;
    public Guid CatalogItemId { get; set; }
    public CatalogItem CatalogItem { get; set; } = null!;
    public Guid UnitOfMeasureId { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; } = null!;
    public decimal Quantity { get; set; }
    public Guid ReceiptStorageLocationId { get; set; }
    public StorageLocation ReceiptStorageLocation { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public string? Note { get; set; }
}

public sealed class CatalogTechnologyOperation : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TechnologyStageId { get; set; }
    public CatalogTechnologyStage TechnologyStage { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid? EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }
    public decimal SetupMinutes { get; set; }
    public decimal RunMinutes { get; set; }
    public decimal LaborMinutes { get; set; }
    public decimal Workers { get; set; } = 1m;
    public string? Note { get; set; }
}

public sealed class CatalogTechnologyMaterialSupplyRouteStep : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TechnologyMaterialId { get; set; }
    public CatalogTechnologyMaterial TechnologyMaterial { get; set; } = null!;
    public int LineNo { get; set; }
    public Guid FromStorageLocationId { get; set; }
    public StorageLocation FromStorageLocation { get; set; } = null!;
    public Guid? ToStorageLocationId { get; set; }
    public StorageLocation? ToStorageLocation { get; set; }
    public bool IsConsumptionPoint { get; set; }
    public TechnologyMaterialMovementKind MovementKind { get; set; } = TechnologyMaterialMovementKind.Transfer;
    public decimal LeadTimeMinutes { get; set; }
    public string? Note { get; set; }
}

public enum TechnologyStageLinkType
{
    FinishToStart = 1,
    StartToStart = 2,
    FinishToFinish = 3,
    StartToFinish = 4
}

public enum TechnologyMaterialConsumptionTrackingMode
{
    NormOnly = 1,
    ActualConsumptionTracked = 2
}

public enum TechnologyMaterialMovementKind
{
    Transfer = 1,
    IssueToProduction = 2,
    InternalMove = 3
}
