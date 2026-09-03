namespace LeanProd.Application.Features.MasterData;

public interface IStorageLocationService
{
    Task<MasterDataPage<StorageLocationSummary>> GetStorageLocationsAsync(
        MasterDataQuery query, Guid? departmentId, Guid? kindId, Guid? typeId, CancellationToken ct);
    Task<StorageLocationDetails?> GetStorageLocationAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<StorageLocationOption>> GetStorageLocationOptionsAsync(CancellationToken ct);
    Task<IReadOnlyCollection<LookupItem>> GetStorageLocationKindsAsync(CancellationToken ct);
    Task<IReadOnlyCollection<LookupItem>> GetStorageLocationTypesAsync(CancellationToken ct);
    Task<MasterDataResult<StorageLocationDetails>> CreateStorageLocationAsync(
        SaveStorageLocationCommand command, CancellationToken ct);
    Task<MasterDataResult<StorageLocationDetails>> UpdateStorageLocationAsync(
        Guid id, SaveStorageLocationCommand command, CancellationToken ct);
    Task<MasterDataResult<bool>> SetStorageLocationActiveAsync(Guid id, bool active, CancellationToken ct);
}
