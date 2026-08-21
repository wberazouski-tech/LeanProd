namespace LeanProd.Application.Features.MasterData;

public interface IMasterDataService
{
    Task<MasterDataPage<DepartmentSummary>> GetDepartmentsAsync(MasterDataQuery query, CancellationToken cancellationToken);
    Task<DepartmentDetails?> GetDepartmentAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OptionItem>> GetDepartmentOptionsAsync(CancellationToken cancellationToken);
    Task<MasterDataResult<DepartmentDetails>> CreateDepartmentAsync(SaveDepartmentCommand command, CancellationToken cancellationToken);
    Task<MasterDataResult<DepartmentDetails>> UpdateDepartmentAsync(Guid id, SaveDepartmentCommand command, CancellationToken cancellationToken);
    Task<MasterDataResult<bool>> SetDepartmentActiveAsync(Guid id, bool active, CancellationToken cancellationToken);

    Task<MasterDataPage<StorageLocationSummary>> GetStorageLocationsAsync(MasterDataQuery query, Guid? departmentId, Guid? kindId, Guid? typeId, CancellationToken cancellationToken);
    Task<StorageLocationDetails?> GetStorageLocationAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<StorageLocationOption>> GetStorageLocationOptionsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CatalogItem>> GetStorageLocationKindsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CatalogItem>> GetStorageLocationTypesAsync(CancellationToken cancellationToken);
    Task<MasterDataResult<StorageLocationDetails>> CreateStorageLocationAsync(SaveStorageLocationCommand command, CancellationToken cancellationToken);
    Task<MasterDataResult<StorageLocationDetails>> UpdateStorageLocationAsync(Guid id, SaveStorageLocationCommand command, CancellationToken cancellationToken);
    Task<MasterDataResult<bool>> SetStorageLocationActiveAsync(Guid id, bool active, CancellationToken cancellationToken);
}
