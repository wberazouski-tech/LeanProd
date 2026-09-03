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
    Task<MasterDataResult<CatalogTechnologyDetails>> UpdateTechnologyAsync(
        Guid id, SaveCatalogTechnologyCommand command, CancellationToken ct);
    Task<MasterDataResult<bool>> SetTechnologyActiveAsync(Guid id, bool active, CancellationToken ct);
    Task<IReadOnlyCollection<TechnologyStageTemplateDetails>> GetStageTemplatesAsync(
        bool activeOnly, CancellationToken ct);
    Task<MasterDataResult<TechnologyStageTemplateDetails>> SaveStageTemplateAsync(
        Guid? id, SaveTechnologyStageTemplateCommand command, CancellationToken ct);
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
    IReadOnlyCollection<CatalogTechnologyStageLinkDto> StageLinks,
    string RowVersion);

public sealed record CatalogTechnologyStageDto(
    Guid Id,
    Guid? StageTemplateId,
    string? StageTemplateName,
    string Code,
    string Name,
    int LineNo,
    decimal? PlannedDurationMinutes,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? EquipmentId,
    string? EquipmentName,
    string? Description,
    IReadOnlyCollection<CatalogTechnologyMaterialDto> Materials,
    IReadOnlyCollection<CatalogTechnologyStageOutputDto> Outputs,
    IReadOnlyCollection<CatalogTechnologyOperationDto> Operations);

public sealed record CatalogTechnologyStageLinkDto(
    Guid Id,
    Guid FromStageId,
    Guid ToStageId,
    TechnologyStageLinkType LinkType,
    decimal LagMinutes);

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
    IReadOnlyCollection<CatalogTechnologyMaterialSupplyRouteStepDto> RouteSteps);

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
    string? Note);

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
    string? Note);

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
    string? Note);

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
    IReadOnlyCollection<SaveCatalogTechnologyStageLinkCommand> StageLinks,
    string? RowVersion);

public sealed record SaveCatalogTechnologyStageCommand(
    Guid? Id,
    Guid? StageTemplateId,
    string Code,
    string Name,
    int LineNo,
    decimal? PlannedDurationMinutes,
    Guid? DepartmentId,
    Guid? EquipmentId,
    string? Description,
    IReadOnlyCollection<SaveCatalogTechnologyMaterialCommand> Materials,
    IReadOnlyCollection<SaveCatalogTechnologyStageOutputCommand> Outputs,
    IReadOnlyCollection<SaveCatalogTechnologyOperationCommand> Operations);

public sealed record SaveCatalogTechnologyStageLinkCommand(
    Guid? Id,
    Guid FromStageId,
    Guid ToStageId,
    TechnologyStageLinkType LinkType,
    decimal LagMinutes);

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
    IReadOnlyCollection<SaveCatalogTechnologyMaterialSupplyRouteStepCommand> RouteSteps);

public sealed record SaveCatalogTechnologyStageOutputCommand(
    Guid? Id,
    Guid CatalogItemId,
    Guid UnitOfMeasureId,
    decimal Quantity,
    Guid ReceiptStorageLocationId,
    bool IsPrimary,
    string? Note);

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
    string? Note);

public sealed record SaveCatalogTechnologyMaterialSupplyRouteStepCommand(
    Guid? Id,
    int LineNo,
    Guid FromStorageLocationId,
    Guid? ToStorageLocationId,
    bool IsConsumptionPoint,
    TechnologyMaterialMovementKind MovementKind,
    decimal LeadTimeMinutes,
    string? Note);

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
