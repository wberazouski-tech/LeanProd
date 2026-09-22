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

public sealed record SaveCatalogTechnologyHeaderRequest(
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
    [Required] string RowVersion);

public sealed record DeleteCatalogTechnologyRowRequest(Guid Id, [Required] string RowVersion);

public sealed record SaveCatalogTechnologyStagesRequest(
    IReadOnlyCollection<SaveCatalogTechnologyStageRowRequest> Stages,
    IReadOnlyCollection<SaveCatalogTechnologyStageTransitionRequest> StageTransitions,
    IReadOnlyCollection<DeleteCatalogTechnologyRowRequest> DeletedStages,
    IReadOnlyCollection<DeleteCatalogTechnologyRowRequest> DeletedStageTransitions);

public sealed record AddNewCatalogTechnologyStageRequest(
    SaveCatalogTechnologyStageRowRequest Stage,
    IReadOnlyCollection<SaveCatalogTechnologyStageTransitionRequest> StageTransitions);

public sealed record SaveCatalogTechnologyStageRowRequest(
    Guid Id,
    Guid? TechnologyStageId,
    [StringLength(50)] string TechnologyStageCode,
    [Required, StringLength(200)] string TechnologyStageName,
    [Range(1, int.MaxValue)] int StageNumber,
    decimal? PlannedDurationMinutes,
    Guid? TechnologyStageDepartmentId,
    Guid? EquipmentId,
    [StringLength(1000)] string? Description,
    string? RowVersion);

public sealed record SaveCatalogTechnologyMaterialsRequest(
    IReadOnlyCollection<SaveCatalogTechnologyMaterialRequest> Materials,
    IReadOnlyCollection<DeleteCatalogTechnologyRowRequest> DeletedMaterials,
    IReadOnlyCollection<DeleteCatalogTechnologyRowRequest> DeletedRouteSteps);

public sealed record SaveCatalogTechnologyOutputsRequest(
    IReadOnlyCollection<SaveCatalogTechnologyStageOutputRequest> Outputs,
    IReadOnlyCollection<DeleteCatalogTechnologyRowRequest> DeletedOutputs);

public sealed record SaveCatalogTechnologyOperationsRequest(
    IReadOnlyCollection<SaveCatalogTechnologyOperationRequest> Operations,
    IReadOnlyCollection<DeleteCatalogTechnologyRowRequest> DeletedOperations);

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
    IReadOnlyCollection<SaveCatalogTechnologyOperationRequest> Operations,
    string? RowVersion = null);

public sealed record SaveCatalogTechnologyStageTransitionRequest(
    Guid? Id,
    Guid FromCatalogTechnologyStageId,
    Guid ToCatalogTechnologyStageId,
    string? RowVersion = null);

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
    IReadOnlyCollection<SaveCatalogTechnologyMaterialSupplyRouteStepRequest> RouteSteps,
    string? RowVersion = null);

public sealed record SaveCatalogTechnologyStageOutputRequest(
    Guid? Id,
    Guid CatalogItemId,
    Guid UnitOfMeasureId,
    decimal Quantity,
    Guid ReceiptStorageLocationId,
    bool IsPrimary,
    [StringLength(1000)] string? Note,
    string? RowVersion = null);

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
    [StringLength(1000)] string? Note,
    string? RowVersion = null);

public sealed record SaveCatalogTechnologyMaterialSupplyRouteStepRequest(
    Guid? Id,
    int LineNo,
    Guid FromStorageLocationId,
    Guid? ToStorageLocationId,
    bool IsConsumptionPoint,
    TechnologyMaterialMovementKind MovementKind,
    decimal LeadTimeMinutes,
    [StringLength(1000)] string? Note,
    string? RowVersion = null);

public sealed record SaveTechnologyStageTemplateRequest(
    [StringLength(50)] string Code,
    [Required, StringLength(200)] string Name,
    [StringLength(1000)] string? Description,
    bool IsActive,
    string? RowVersion);
