using System.Text.Json.Serialization;
using LeanProd.Domain.MasterData;

namespace LeanProd.Application.Features.MasterData;

public interface ICatalogItemService
{
    Task<MasterDataPage<CatalogItemSummary>> GetItemsAsync(CatalogItemType type, MasterDataQuery query, CancellationToken ct);
    Task<CatalogItemDetails?> GetItemAsync(Guid id, CancellationToken ct);
    Task<MasterDataResult<CatalogItemDetails>> CreateItemAsync(SaveCatalogItemCommand command, CancellationToken ct);
    Task<MasterDataResult<CatalogItemDetails>> UpdateItemAsync(Guid id, SaveCatalogItemCommand command, CancellationToken ct);
    Task<MasterDataResult<CatalogItemDetails>> ChangeItemClassAsync(Guid id, Guid catalogItemClassId, CancellationToken ct);
    Task<MasterDataResult<bool>> SetItemActiveAsync(Guid id, bool active, CancellationToken ct);
}

public sealed record CatalogItemSummary(Guid Id, string WorkingName, string? FullName,
    string? ArticleNumber, CatalogItemType Type, Guid BaseUnitOfMeasureId,
    string BaseUnitName, string? BaseUnitSymbol, Guid CatalogItemClassId,
    string CatalogItemClassCode, string CatalogItemClassName,
    [property: JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)] decimal Cost, bool IsActive);

public sealed record CatalogItemDetails(Guid Id, string WorkingName, string? FullName,
    string? ArticleNumber, CatalogItemType Type, Guid BaseUnitOfMeasureId,
    string BaseUnitName, string? BaseUnitSymbol, Guid CatalogItemClassId,
    string CatalogItemClassCode, string CatalogItemClassName,
    [property: JsonNumberHandling(JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString)] decimal Cost,
    string? Description, bool IsActive, string RowVersion);

public sealed record SaveCatalogItemCommand(string WorkingName, string? FullName,
    string? ArticleNumber, CatalogItemType Type, Guid BaseUnitOfMeasureId,
    Guid CatalogItemClassId, decimal Cost, string? Description, string? RowVersion);
