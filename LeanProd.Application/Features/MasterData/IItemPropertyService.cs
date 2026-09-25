using System.Text.Json.Serialization;
using LeanProd.Domain.MasterData;

namespace LeanProd.Application.Features.MasterData;

public sealed record PropertyOptionDto(Guid? Id, string Label);
public sealed record SavePropertyCommand(string Name, ItemPropertyType Type,
    int? DecimalPlaces, int? MaxLength,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)] decimal? Minimum,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)] decimal? Maximum,
    bool IsBatchProperty, bool IsActive, IReadOnlyList<PropertyOptionDto> Options, string? RowVersion);
public sealed record PropertyDefinitionDto(Guid Id, Guid CatalogItemClassId, string Name,
    ItemPropertyType Type, int? DecimalPlaces, int? MaxLength,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)] decimal? Minimum,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)] decimal? Maximum,
    bool IsBatchProperty, bool IsActive, IReadOnlyList<PropertyOptionDto> Options, string RowVersion, bool IsInUse = false);
public sealed record PropertyValueDto(Guid PropertyId,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)] decimal? Number = null,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)] decimal? Upper = null,
    string? Text = null, bool? Boolean = null, Guid? OptionId = null);
public sealed record PropertyValuesDto(string RowVersion, IReadOnlyList<PropertyDefinitionDto> Definitions,
    IReadOnlyList<PropertyValueDto> Values, BatchDto? Batch = null, bool IsEditable = true);
public sealed record SavePropertyValuesCommand(string RowVersion, IReadOnlyList<PropertyValueDto> Values);
public sealed record SaveBatchCommand(string Number, DateOnly ReceiptDate, string ReceiptReference, string? RowVersion);
public sealed record BatchDto(Guid Id, Guid CatalogItemId, string Number, DateOnly ReceiptDate,
    string ReceiptReference, string RowVersion);

public interface IItemPropertyService
{
    Task<MasterDataResult<IReadOnlyList<PropertyDefinitionDto>>> Definitions(Guid classId, CancellationToken ct);
    Task<MasterDataResult<PropertyDefinitionDto>> SaveDefinition(Guid classId, Guid? id, SavePropertyCommand command, CancellationToken ct);
    Task<MasterDataResult<PropertyValuesDto>> Values(Guid itemId, Guid? batchId, CancellationToken ct);
    Task<MasterDataResult<PropertyValuesDto>> SaveValues(Guid itemId, Guid? batchId, SavePropertyValuesCommand command, CancellationToken ct);
    Task<MasterDataResult<MasterDataPage<BatchDto>>> Batches(Guid itemId, int page, CancellationToken ct);
    Task<MasterDataResult<BatchDto>> SaveBatch(Guid itemId, Guid? id, SaveBatchCommand command, CancellationToken ct);
}
