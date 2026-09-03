using LeanProd.Domain.MasterData;

namespace LeanProd.Application.Features.MasterData;

public interface ICatalogItemClassService
{
    Task<MasterDataPage<CatalogItemClassSummary>> GetCatalogItemClassesAsync(
        CatalogItemType type, MasterDataQuery query, bool? isGroup, CancellationToken ct);
    Task<CatalogItemClassDetails?> GetCatalogItemClassAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<CatalogItemClassOption>> GetCatalogItemClassOptionsAsync(
        CatalogItemType type, bool activeOnly, CancellationToken ct);
    Task<MasterDataResult<CatalogItemClassDetails>> CreateCatalogItemClassAsync(
        SaveCatalogItemClassCommand command, CancellationToken ct);
    Task<MasterDataResult<CatalogItemClassDetails>> UpdateCatalogItemClassAsync(
        Guid id, SaveCatalogItemClassCommand command, CancellationToken ct);
    Task<MasterDataResult<bool>> SetCatalogItemClassActiveAsync(Guid id, bool active, CancellationToken ct);
}
