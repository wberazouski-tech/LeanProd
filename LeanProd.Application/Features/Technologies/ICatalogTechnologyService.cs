using LeanProd.Application.Features.MasterData;
using LeanProd.Domain.MasterData;
using LeanProd.Domain.Technologies;

namespace LeanProd.Application.Features.Technologies;

public interface ICatalogTechnologyService
{
    Task<MasterDataPage<CatalogTechnologySummary>> GetTechnologiesAsync(
        CatalogTechnologyQuery query, CancellationToken ct);
    Task<CatalogTechnologyDetails?> GetTechnologyAsync(Guid id, CancellationToken ct);
    Task<MasterDataResult<CatalogTechnologyDetails>> CreateTechnologyAsync(
        SaveCatalogTechnologyCommand command, CancellationToken ct);
    Task<MasterDataResult<CatalogTechnologyDetails>> UpdateTechnologyHeaderAsync(
        Guid id, SaveCatalogTechnologyHeaderCommand command, CancellationToken ct);
    Task<MasterDataResult<CatalogTechnologyDetails>> SaveTechnologyStagesAsync(
        Guid id, SaveCatalogTechnologyStagesCommand command, CancellationToken ct);
    Task<MasterDataResult<CatalogTechnologyDetails>> AddNewTechnologyStageAsync(
        Guid id, AddNewCatalogTechnologyStageCommand command, CancellationToken ct);
    Task<MasterDataResult<CatalogTechnologyDetails>> AddExistingTechnologyStageAsync(
        Guid id, SaveCatalogTechnologyStageRowCommand command, CancellationToken ct);
    Task<MasterDataResult<CatalogTechnologyDetails>> SaveStageMaterialsAsync(
        Guid technologyId, Guid stageId, SaveCatalogTechnologyMaterialsCommand command, CancellationToken ct);
    Task<MasterDataResult<CatalogTechnologyDetails>> SaveStageOutputsAsync(
        Guid technologyId, Guid stageId, SaveCatalogTechnologyOutputsCommand command, CancellationToken ct);
    Task<MasterDataResult<CatalogTechnologyDetails>> SaveStageOperationsAsync(
        Guid technologyId, Guid stageId, SaveCatalogTechnologyOperationsCommand command, CancellationToken ct);
    Task<MasterDataResult<bool>> SetTechnologyActiveAsync(Guid id, bool active, CancellationToken ct);
    Task<IReadOnlyCollection<TechnologyStageTemplateDetails>> GetStageTemplatesAsync(
        bool activeOnly, CancellationToken ct);
    Task<MasterDataResult<TechnologyStageTemplateDetails>> SaveStageTemplateAsync(
        Guid? id, SaveTechnologyStageTemplateCommand command, CancellationToken ct);
    Task<IReadOnlyCollection<TechnologyStageDetails>> GetTechnologyStagesAsync(bool activeOnly, CancellationToken ct);
    Task<bool> HasTechnologyStageDuplicateAsync(string name, Guid departmentId, CancellationToken ct);
}

public sealed record CatalogTechnologyQuery(
    int Page,
    int PageSize,
    string? Search,
    bool? IsActive,
    Guid? CatalogItemId,
    Guid? CatalogItemClassId);

public sealed record CatalogTechnologySummary(
    Guid Id,
    string Code,
    string Name,
    Guid? CatalogItemId,
    string? CatalogItemName,
    Guid? CatalogItemClassId,
    string? CatalogItemClassCode,
    string? CatalogItemClassName,
    int VersionNo,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    bool IsDefault,
    CatalogTechnologyStatus Status,
    bool IsActive);

public sealed record CatalogTechnologyDetails(
    Guid Id,
    string Code,
    string Name,
    Guid? CatalogItemId,
    string? CatalogItemName,
    Guid? CatalogItemClassId,
    string? CatalogItemClassCode,
    string? CatalogItemClassName,
    int VersionNo,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    bool IsDefault,
    CatalogTechnologyStatus Status,
    string? Description,
    bool IsActive,
    IReadOnlyCollection<CatalogTechnologyStageDto> Stages,
    IReadOnlyCollection<CatalogTechnologyStageTransitionDto> StageTransitions,
    string RowVersion);

public sealed record CatalogTechnologyStageDto(
    Guid Id,
    Guid TechnologyStageId,
    string TechnologyStageCode,
    string TechnologyStageName,
    int StageNumber,
    decimal? PlannedDurationMinutes,
    Guid? TechnologyStageDepartmentId,
    string? TechnologyStageDepartmentName,
    Guid? EquipmentId,
    string? EquipmentName,
    string? Description,
    IReadOnlyCollection<CatalogTechnologyMaterialDto> Materials,
    IReadOnlyCollection<CatalogTechnologyStageOutputDto> Outputs,
    IReadOnlyCollection<CatalogTechnologyOperationDto> Operations,
    string RowVersion);

public sealed record CatalogTechnologyStageTransitionDto(
    Guid Id,
    Guid FromCatalogTechnologyStageId,
    Guid ToCatalogTechnologyStageId,
    string RowVersion);

public sealed record CatalogTechnologyMaterialDto(
    Guid Id,
    Guid CatalogItemId,
    string CatalogItemName,
    Guid UnitOfMeasureId,
    string UnitOfMeasureName,
    decimal Quantity,
    TechnologyMaterialConsumptionTrackingMode ConsumptionTrackingMode,
    Guid? DefaultSourceStorageLocationId,
    string? DefaultSourceStorageLocationName,
    decimal ScrapPercent,
    bool IsOptional,
    string? Note,
    IReadOnlyCollection<CatalogTechnologyMaterialSupplyRouteStepDto> RouteSteps,
    string RowVersion);

public sealed record CatalogTechnologyStageOutputDto(
    Guid Id,
    Guid CatalogItemId,
    string CatalogItemName,
    Guid UnitOfMeasureId,
    string UnitOfMeasureName,
    decimal Quantity,
    Guid ReceiptStorageLocationId,
    string ReceiptStorageLocationName,
    bool IsPrimary,
    string? Note,
    string RowVersion);

public sealed record CatalogTechnologyOperationDto(
    Guid Id,
    string Code,
    string Name,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? EquipmentId,
    string? EquipmentName,
    decimal SetupMinutes,
    decimal RunMinutes,
    decimal LaborMinutes,
    decimal Workers,
    string? Note,
    string RowVersion);

public sealed record CatalogTechnologyMaterialSupplyRouteStepDto(
    Guid Id,
    int LineNo,
    Guid FromStorageLocationId,
    string FromStorageLocationName,
    Guid? ToStorageLocationId,
    string? ToStorageLocationName,
    bool IsConsumptionPoint,
    TechnologyMaterialMovementKind MovementKind,
    decimal LeadTimeMinutes,
    string? Note,
    string RowVersion);

public sealed record SaveCatalogTechnologyCommand(
    string Code,
    string Name,
    Guid? CatalogItemId,
    Guid? CatalogItemClassId,
    int VersionNo,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    bool IsDefault,
    CatalogTechnologyStatus Status,
    string? Description,
    IReadOnlyCollection<SaveCatalogTechnologyStageCommand> Stages,
    IReadOnlyCollection<SaveCatalogTechnologyStageTransitionCommand> StageTransitions,
    string? RowVersion);

public sealed record SaveCatalogTechnologyHeaderCommand(
    string Code,
    string Name,
    Guid? CatalogItemId,
    Guid? CatalogItemClassId,
    int VersionNo,
    DateOnly? ValidFrom,
    DateOnly? ValidTo,
    bool IsDefault,
    CatalogTechnologyStatus Status,
    string? Description,
    string? RowVersion);

public sealed record DeleteCatalogTechnologyRowCommand(Guid Id, string RowVersion);

public sealed record SaveCatalogTechnologyStagesCommand(
    IReadOnlyCollection<SaveCatalogTechnologyStageRowCommand> Stages,
    IReadOnlyCollection<SaveCatalogTechnologyStageTransitionCommand> StageTransitions,
    IReadOnlyCollection<DeleteCatalogTechnologyRowCommand> DeletedStages,
    IReadOnlyCollection<DeleteCatalogTechnologyRowCommand> DeletedStageTransitions);

public sealed record AddNewCatalogTechnologyStageCommand(
    SaveCatalogTechnologyStageRowCommand Stage,
    IReadOnlyCollection<SaveCatalogTechnologyStageTransitionCommand> StageTransitions);

public sealed record SaveCatalogTechnologyStageRowCommand(
    Guid Id,
    Guid? TechnologyStageId,
    string TechnologyStageCode,
    string TechnologyStageName,
    int StageNumber,
    decimal? PlannedDurationMinutes,
    Guid? TechnologyStageDepartmentId,
    Guid? EquipmentId,
    string? Description,
    string? RowVersion);

public sealed record SaveCatalogTechnologyMaterialsCommand(
    IReadOnlyCollection<SaveCatalogTechnologyMaterialCommand> Materials,
    IReadOnlyCollection<DeleteCatalogTechnologyRowCommand> DeletedMaterials,
    IReadOnlyCollection<DeleteCatalogTechnologyRowCommand> DeletedRouteSteps);

public sealed record SaveCatalogTechnologyOutputsCommand(
    IReadOnlyCollection<SaveCatalogTechnologyStageOutputCommand> Outputs,
    IReadOnlyCollection<DeleteCatalogTechnologyRowCommand> DeletedOutputs);

public sealed record SaveCatalogTechnologyOperationsCommand(
    IReadOnlyCollection<SaveCatalogTechnologyOperationCommand> Operations,
    IReadOnlyCollection<DeleteCatalogTechnologyRowCommand> DeletedOperations);

public sealed record SaveCatalogTechnologyStageCommand(
    Guid? Id,
    Guid? TechnologyStageId,
    string TechnologyStageCode,
    string TechnologyStageName,
    int StageNumber,
    decimal? PlannedDurationMinutes,
    Guid? TechnologyStageDepartmentId,
    Guid? EquipmentId,
    string? Description,
    IReadOnlyCollection<SaveCatalogTechnologyMaterialCommand> Materials,
    IReadOnlyCollection<SaveCatalogTechnologyStageOutputCommand> Outputs,
    IReadOnlyCollection<SaveCatalogTechnologyOperationCommand> Operations,
    string? RowVersion = null);

public sealed record SaveCatalogTechnologyStageTransitionCommand(
    Guid? Id,
    Guid FromCatalogTechnologyStageId,
    Guid ToCatalogTechnologyStageId,
    string? RowVersion = null);

public sealed record TechnologyStageDetails(Guid Id, string Code, string Name, string? Description, bool IsActive,
    Guid? DepartmentId, string? DepartmentName);

public sealed record SaveCatalogTechnologyMaterialCommand(
    Guid? Id,
    Guid CatalogItemId,
    Guid UnitOfMeasureId,
    decimal Quantity,
    TechnologyMaterialConsumptionTrackingMode ConsumptionTrackingMode,
    Guid? DefaultSourceStorageLocationId,
    decimal ScrapPercent,
    bool IsOptional,
    string? Note,
    IReadOnlyCollection<SaveCatalogTechnologyMaterialSupplyRouteStepCommand> RouteSteps,
    string? RowVersion = null);

public sealed record SaveCatalogTechnologyStageOutputCommand(
    Guid? Id,
    Guid CatalogItemId,
    Guid UnitOfMeasureId,
    decimal Quantity,
    Guid ReceiptStorageLocationId,
    bool IsPrimary,
    string? Note,
    string? RowVersion = null);

public sealed record SaveCatalogTechnologyOperationCommand(
    Guid? Id,
    string Code,
    string Name,
    Guid? DepartmentId,
    Guid? EquipmentId,
    decimal SetupMinutes,
    decimal RunMinutes,
    decimal LaborMinutes,
    decimal Workers,
    string? Note,
    string? RowVersion = null);

public sealed record SaveCatalogTechnologyMaterialSupplyRouteStepCommand(
    Guid? Id,
    int LineNo,
    Guid FromStorageLocationId,
    Guid? ToStorageLocationId,
    bool IsConsumptionPoint,
    TechnologyMaterialMovementKind MovementKind,
    decimal LeadTimeMinutes,
    string? Note,
    string? RowVersion = null);

public sealed record TechnologyStageTemplateDetails(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    string RowVersion);

public sealed record SaveTechnologyStageTemplateCommand(
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    string? RowVersion);
