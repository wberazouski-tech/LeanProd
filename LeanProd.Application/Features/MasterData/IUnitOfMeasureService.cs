namespace LeanProd.Application.Features.MasterData;

public interface IUnitOfMeasureService
{
    Task<MasterDataPage<UnitOfMeasureSummary>> GetUnitsAsync(MasterDataQuery query, string languageCode, CancellationToken ct);
    Task<UnitOfMeasureDetails?> GetUnitAsync(Guid id, string languageCode, CancellationToken ct);
    Task<IReadOnlyCollection<UnitOfMeasureSummary>> GetUnitOptionsAsync(string languageCode, CancellationToken ct);
    Task<IReadOnlyCollection<UnitCatalogOption>> GetCatalogAsync(string? search, CancellationToken ct);
    Task<MasterDataResult<UnitOfMeasureDetails>> CreateUnitAsync(CreateUnitOfMeasureCommand command, CancellationToken ct);
    Task<MasterDataResult<UnitOfMeasureDetails>> UpdateUnitAsync(Guid id, UpdateUnitOfMeasureCommand command, CancellationToken ct);
    Task<MasterDataResult<bool>> SetUnitActiveAsync(Guid id, bool active, CancellationToken ct);
    Task<IReadOnlyCollection<UnitConversionDetails>> GetConversionsAsync(CancellationToken ct);
    Task<MasterDataResult<UnitConversionDetails>> CreateConversionAsync(CreateUnitConversionCommand command, CancellationToken ct);
    Task<MasterDataResult<bool>> DeleteConversionAsync(Guid id, CancellationToken ct);
    Task<MasterDataResult<UnitConversionCalculation>> ConvertAsync(ConvertUnitCommand command, CancellationToken ct);
}

public sealed record UnitCatalogOption(string Code, string Name, string? Symbol, string LetterCode, bool IsAdded);
public sealed record UnitOfMeasureSummary(Guid Id, string Code, string DisplayName, string Name, string? Symbol,
    string LetterCode, string QuantityType, byte DecimalPlaces, bool IsActive);
public sealed record UnitTranslation(string LanguageCode, string Name);
public sealed record UnitOfMeasureDetails(Guid Id, string Code, string DisplayName, string Name, string? Symbol,
    string LetterCode, string QuantityType, byte DecimalPlaces, bool IsActive,
    IReadOnlyCollection<UnitTranslation> Translations, string RowVersion);
public sealed record CreateUnitOfMeasureCommand(string CatalogCode, string QuantityType, byte DecimalPlaces,
    string LanguageCode, string? LocalizedName);
public sealed record UpdateUnitOfMeasureCommand(string QuantityType, byte DecimalPlaces,
    string LanguageCode, string? LocalizedName, string? RowVersion);
public sealed record UnitConversionDetails(Guid Id, Guid FromUnitId, string FromUnitName, string FromLetterCode,
    Guid ToUnitId, string ToUnitName, string ToLetterCode, decimal Multiplier, decimal Offset, string RowVersion);
public sealed record CreateUnitConversionCommand(Guid FromUnitId, Guid ToUnitId, decimal Multiplier, decimal Offset);
public sealed record ConvertUnitCommand(Guid FromUnitId, Guid ToUnitId, decimal Value);
public sealed record UnitConversionCalculation(Guid FromUnitId, Guid ToUnitId, decimal SourceValue, decimal Result);
