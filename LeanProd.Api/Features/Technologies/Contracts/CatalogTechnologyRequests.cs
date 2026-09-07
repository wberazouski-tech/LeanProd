using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LeanProd.Domain.Technologies;

namespace LeanProd.Api.Features.Technologies.Contracts;

public sealed record SaveCatalogTechnologyRequest(
    [StringLength(50)] string Code,
    [Required, StringLength(200)] string Name,
    Guid? CatalogItemId,
    Guid? CatalogItemClassId,
    int VersionNo,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    bool IsDefault,
    [Required] CatalogTechnologyStatus? Status,
    [StringLength(1000)] string? Description,
    [Required] IReadOnlyCollection<SaveCatalogTechnologyStageRequest> Stages,
    IReadOnlyCollection<SaveCatalogTechnologyStageTransitionRequest> StageTransitions,
    string? RowVersion);

public sealed record SaveCatalogTechnologyStageRequest(
    Guid? Id,
    Guid? TechnologyStageId,
    [StringLength(50)] string TechnologyStageCode,
    [Required, StringLength(200)] string TechnologyStageName,
    [Range(1, int.MaxValue)] int StageNumber,
    decimal? PlannedDurationMinutes,
    Guid? TechnologyStageDepartmentId,
    Guid? EquipmentId,
    [StringLength(1000)] string? Description,
    IReadOnlyCollection<SaveCatalogTechnologyMaterialRequest> Materials,
    IReadOnlyCollection<SaveCatalogTechnologyStageOutputRequest> Outputs,
    IReadOnlyCollection<SaveCatalogTechnologyOperationRequest> Operations);

public sealed record SaveCatalogTechnologyStageTransitionRequest(
    Guid? Id,
    Guid FromCatalogTechnologyStageId,
    Guid ToCatalogTechnologyStageId);

public sealed record SaveCatalogTechnologyMaterialRequest(
    Guid? Id,
    Guid CatalogItemId,
    Guid UnitOfMeasureId,
    decimal Quantity,
    TechnologyMaterialConsumptionTrackingMode ConsumptionTrackingMode,
    Guid? DefaultSourceStorageLocationId,
    decimal ScrapPercent,
    bool IsOptional,
    [StringLength(1000)] string? Note,
    IReadOnlyCollection<SaveCatalogTechnologyMaterialSupplyRouteStepRequest> RouteSteps);

public sealed record SaveCatalogTechnologyStageOutputRequest(
    Guid? Id,
    Guid CatalogItemId,
    Guid UnitOfMeasureId,
    decimal Quantity,
    Guid ReceiptStorageLocationId,
    bool IsPrimary,
    [StringLength(1000)] string? Note);

public sealed record SaveCatalogTechnologyOperationRequest(
    Guid? Id,
    [StringLength(50)] string Code,
    [Required, StringLength(200)] string Name,
    Guid? DepartmentId,
    Guid? EquipmentId,
    decimal SetupMinutes,
    decimal RunMinutes,
    decimal LaborMinutes,
    decimal Workers,
    [StringLength(1000)] string? Note);

public sealed record SaveCatalogTechnologyMaterialSupplyRouteStepRequest(
    Guid? Id,
    int LineNo,
    Guid FromStorageLocationId,
    Guid? ToStorageLocationId,
    bool IsConsumptionPoint,
    TechnologyMaterialMovementKind MovementKind,
    decimal LeadTimeMinutes,
    [StringLength(1000)] string? Note);

public sealed record SaveTechnologyStageTemplateRequest(
    [StringLength(50)] string Code,
    [Required, StringLength(200)] string Name,
    [StringLength(1000)] string? Description,
    bool IsActive,
    string? RowVersion);
