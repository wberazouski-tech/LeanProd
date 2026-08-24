using LeanProd.Domain.MasterData;

namespace LeanProd.Application.Features.MasterData;

public interface IMasterDataService
{
    Task<MasterDataPage<CatalogItemClassSummary>> GetCatalogItemClassesAsync(CatalogItemType type, MasterDataQuery query, bool? isGroup, CancellationToken cancellationToken);
    Task<CatalogItemClassDetails?> GetCatalogItemClassAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CatalogItemClassOption>> GetCatalogItemClassOptionsAsync(CatalogItemType type, bool activeOnly, CancellationToken cancellationToken);
    Task<MasterDataResult<CatalogItemClassDetails>> CreateCatalogItemClassAsync(SaveCatalogItemClassCommand command, CancellationToken cancellationToken);
    Task<MasterDataResult<CatalogItemClassDetails>> UpdateCatalogItemClassAsync(Guid id, SaveCatalogItemClassCommand command, CancellationToken cancellationToken);
    Task<MasterDataResult<bool>> SetCatalogItemClassActiveAsync(Guid id, bool active, CancellationToken cancellationToken);

    Task<MasterDataPage<DepartmentSummary>> GetDepartmentsAsync(MasterDataQuery query, CancellationToken cancellationToken);
    Task<DepartmentDetails?> GetDepartmentAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OptionItem>> GetDepartmentOptionsAsync(CancellationToken cancellationToken);
    Task<MasterDataResult<DepartmentDetails>> CreateDepartmentAsync(SaveDepartmentCommand command, CancellationToken cancellationToken);
    Task<MasterDataResult<DepartmentDetails>> UpdateDepartmentAsync(Guid id, SaveDepartmentCommand command, CancellationToken cancellationToken);
    Task<MasterDataResult<bool>> SetDepartmentActiveAsync(Guid id, bool active, CancellationToken cancellationToken);

    Task<MasterDataPage<StorageLocationSummary>> GetStorageLocationsAsync(MasterDataQuery query, Guid? departmentId, Guid? kindId, Guid? typeId, CancellationToken cancellationToken);
    Task<StorageLocationDetails?> GetStorageLocationAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<StorageLocationOption>> GetStorageLocationOptionsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LookupItem>> GetStorageLocationKindsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LookupItem>> GetStorageLocationTypesAsync(CancellationToken cancellationToken);
    Task<MasterDataResult<StorageLocationDetails>> CreateStorageLocationAsync(SaveStorageLocationCommand command, CancellationToken cancellationToken);
    Task<MasterDataResult<StorageLocationDetails>> UpdateStorageLocationAsync(Guid id, SaveStorageLocationCommand command, CancellationToken cancellationToken);
    Task<MasterDataResult<bool>> SetStorageLocationActiveAsync(Guid id, bool active, CancellationToken cancellationToken);
}
