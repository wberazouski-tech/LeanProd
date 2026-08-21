using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.MasterData;
using LeanProd.Domain.MasterData;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.MasterData;

public sealed class UnitOfMeasureService(LeanProdDbContext db, IInternationalUnitCatalog catalog) : IUnitOfMeasureService
{
    private static readonly HashSet<string> QuantityTypes = new(StringComparer.OrdinalIgnoreCase)
        { "Mass", "Length", "Area", "Volume", "Time", "Temperature", "Count", "Other" };

    public async Task<MasterDataPage<UnitOfMeasureSummary>> GetUnitsAsync(MasterDataQuery query, string languageCode, CancellationToken ct)
    {
        var source = db.UnitOfMeasures.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Code.Contains(search) || x.Name.Contains(search) ||
                x.LetterCode.Contains(search) || x.Translations.Any(t => t.Name.Contains(search)));
        }
        if (query.IsActive is not null) source = source.Where(x => x.IsActive == query.IsActive);
        var total = await source.CountAsync(ct);
        var units = await source.Include(x => x.Translations).OrderBy(x => x.Code)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToArrayAsync(ct);
        return new(units.Select(x => Summary(x, languageCode)).ToArray(), query.Page, query.PageSize, total);
    }

    public async Task<UnitOfMeasureDetails?> GetUnitAsync(Guid id, string languageCode, CancellationToken ct)
    {
        var unit = await db.UnitOfMeasures.AsNoTracking().Include(x => x.Translations).SingleOrDefaultAsync(x => x.Id == id, ct);
        return unit is null ? null : Details(unit, languageCode);
    }

    public async Task<IReadOnlyCollection<UnitOfMeasureSummary>> GetUnitOptionsAsync(string languageCode, CancellationToken ct)
    {
        var units = await db.UnitOfMeasures.AsNoTracking().Where(x => x.IsActive).Include(x => x.Translations)
            .OrderBy(x => x.Code).ToArrayAsync(ct);
        return units.Select(x => Summary(x, languageCode)).ToArray();
    }

    public async Task<IReadOnlyCollection<UnitCatalogOption>> GetCatalogAsync(string? search, CancellationToken ct)
    {
        var added = (await db.UnitOfMeasures.AsNoTracking().Select(x => x.Code).ToArrayAsync(ct)).ToHashSet();
        var items = catalog.Items.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            items = items.Where(x => x.Code.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                x.Name.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                x.LetterCode.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                (x.Symbol?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        return items.OrderBy(x => x.Code).Take(100)
            .Select(x => new UnitCatalogOption(x.Code, x.Name, x.Symbol, x.LetterCode, added.Contains(x.Code))).ToArray();
    }

    public async Task<MasterDataResult<UnitOfMeasureDetails>> CreateUnitAsync(CreateUnitOfMeasureCommand command, CancellationToken ct)
    {
        var source = catalog.Find(command.CatalogCode);
        if (source is null) return Validation<UnitOfMeasureDetails>("Select a unit from the international catalog.");
        var validation = ValidateValues(command.QuantityType, command.DecimalPlaces, command.LanguageCode, command.LocalizedName);
        if (validation is not null) return Validation<UnitOfMeasureDetails>(validation);
        if (await db.UnitOfMeasures.AnyAsync(x => x.Code == source.Code || x.LetterCode == source.LetterCode, ct))
            return Conflict<UnitOfMeasureDetails>("This international unit has already been added.");
        var unit = new UnitOfMeasure
        {
            Code = source.Code, Name = source.Name, Symbol = source.Symbol, LetterCode = source.LetterCode,
            QuantityType = NormalizeQuantityType(command.QuantityType), DecimalPlaces = command.DecimalPlaces
        };
        SetTranslation(unit, command.LanguageCode, command.LocalizedName);
        db.UnitOfMeasures.Add(unit);
        try { await db.SaveChangesAsync(ct); return MasterDataResult<UnitOfMeasureDetails>.Success(Details(unit, command.LanguageCode)); }
        catch (DbUpdateException) { return Conflict<UnitOfMeasureDetails>("This international unit has already been added."); }
    }

    public async Task<MasterDataResult<UnitOfMeasureDetails>> UpdateUnitAsync(Guid id, UpdateUnitOfMeasureCommand command, CancellationToken ct)
    {
        var unit = await db.UnitOfMeasures.Include(x => x.Translations).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (unit is null) return NotFound<UnitOfMeasureDetails>();
        var validation = ValidateValues(command.QuantityType, command.DecimalPlaces, command.LanguageCode, command.LocalizedName);
        if (validation is not null) return Validation<UnitOfMeasureDetails>(validation);
        if (!SetVersion(unit, command.RowVersion)) return Validation<UnitOfMeasureDetails>("Row version is required.");
        unit.QuantityType = NormalizeQuantityType(command.QuantityType);
        unit.DecimalPlaces = command.DecimalPlaces;
        SetTranslation(unit, command.LanguageCode, command.LocalizedName);
        try { await db.SaveChangesAsync(ct); return MasterDataResult<UnitOfMeasureDetails>.Success(Details(unit, command.LanguageCode)); }
        catch (DbUpdateConcurrencyException) { return Conflict<UnitOfMeasureDetails>("The unit was changed by another request."); }
    }

    public async Task<MasterDataResult<bool>> SetUnitActiveAsync(Guid id, bool active, CancellationToken ct)
    {
        var unit = await db.UnitOfMeasures.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (unit is null) return NotFound<bool>();
        if (!active && await db.UnitOfMeasureConversions.AnyAsync(x => x.FromUnitId == id || x.ToUnitId == id, ct))
            return MasterDataResult<bool>.Failure(MasterDataError.Dependency, "Remove conversion rules before deactivating the unit.");
        unit.IsActive = active;
        await db.SaveChangesAsync(ct);
        return MasterDataResult<bool>.Success(true);
    }

    public async Task<IReadOnlyCollection<UnitConversionDetails>> GetConversionsAsync(CancellationToken ct) =>
        await db.UnitOfMeasureConversions.AsNoTracking().OrderBy(x => x.FromUnit.Code).ThenBy(x => x.ToUnit.Code)
            .Select(x => new UnitConversionDetails(x.Id, x.FromUnitId, x.FromUnit.Name, x.FromUnit.LetterCode,
                x.ToUnitId, x.ToUnit.Name, x.ToUnit.LetterCode, x.Multiplier, x.Offset,
                Convert.ToBase64String(x.RowVersion))).ToArrayAsync(ct);

    public async Task<MasterDataResult<UnitConversionDetails>> CreateConversionAsync(CreateUnitConversionCommand command, CancellationToken ct)
    {
        if (command.FromUnitId == command.ToUnitId) return Validation<UnitConversionDetails>("Choose two different units.");
        if (command.Multiplier <= 0) return Validation<UnitConversionDetails>("Multiplier must be greater than zero.");
        var units = await db.UnitOfMeasures.Where(x => x.Id == command.FromUnitId || x.Id == command.ToUnitId).ToArrayAsync(ct);
        if (units.Length != 2 || units.Any(x => !x.IsActive)) return Validation<UnitConversionDetails>("Both units must exist and be active.");
        var from = units.Single(x => x.Id == command.FromUnitId); var to = units.Single(x => x.Id == command.ToUnitId);
        if (!string.Equals(from.QuantityType, to.QuantityType, StringComparison.OrdinalIgnoreCase))
            return Validation<UnitConversionDetails>("Units must have the same quantity type.");
        if (await db.UnitOfMeasureConversions.AnyAsync(x =>
                (x.FromUnitId == command.FromUnitId && x.ToUnitId == command.ToUnitId) ||
                (x.FromUnitId == command.ToUnitId && x.ToUnitId == command.FromUnitId), ct))
            return Conflict<UnitConversionDetails>("A conversion between these units already exists.");
        var conversion = new UnitOfMeasureConversion { FromUnitId = from.Id, ToUnitId = to.Id, Multiplier = command.Multiplier, Offset = command.Offset };
        db.UnitOfMeasureConversions.Add(conversion);
        await db.SaveChangesAsync(ct);
        return MasterDataResult<UnitConversionDetails>.Success(new(conversion.Id, from.Id, from.Name, from.LetterCode,
            to.Id, to.Name, to.LetterCode, conversion.Multiplier, conversion.Offset, Convert.ToBase64String(conversion.RowVersion)));
    }

    public async Task<MasterDataResult<bool>> DeleteConversionAsync(Guid id, CancellationToken ct)
    {
        var conversion = await db.UnitOfMeasureConversions.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (conversion is null) return NotFound<bool>();
        db.Remove(conversion); await db.SaveChangesAsync(ct); return MasterDataResult<bool>.Success(true);
    }

    public async Task<MasterDataResult<UnitConversionCalculation>> ConvertAsync(ConvertUnitCommand command, CancellationToken ct)
    {
        if (command.FromUnitId == command.ToUnitId)
            return MasterDataResult<UnitConversionCalculation>.Success(new(command.FromUnitId, command.ToUnitId, command.Value, command.Value));
        var conversion = await db.UnitOfMeasureConversions.AsNoTracking()
            .SingleOrDefaultAsync(x =>
                (x.FromUnitId == command.FromUnitId && x.ToUnitId == command.ToUnitId) ||
                (x.FromUnitId == command.ToUnitId && x.ToUnitId == command.FromUnitId), ct);
        if (conversion is null) return NotFound<UnitConversionCalculation>();
        var targetPrecision = await db.UnitOfMeasures.AsNoTracking().Where(x => x.Id == command.ToUnitId)
            .Select(x => x.DecimalPlaces).SingleAsync(ct);
        var direct = conversion.FromUnitId == command.FromUnitId;
        var raw = direct
            ? command.Value * conversion.Multiplier + conversion.Offset
            : (command.Value - conversion.Offset) / conversion.Multiplier;
        var result = decimal.Round(raw, targetPrecision, MidpointRounding.AwayFromZero);
        return MasterDataResult<UnitConversionCalculation>.Success(new(command.FromUnitId, command.ToUnitId, command.Value, result));
    }

    private static string? ValidateValues(string quantityType, byte decimalPlaces, string languageCode, string? localizedName)
    {
        if (!QuantityTypes.Contains(quantityType)) return "Select a valid quantity type.";
        if (decimalPlaces > 6) return "Decimal places must be between 0 and 6.";
        if (!SupportedLanguages.All.Contains(languageCode)) return "The language is not supported.";
        if (!languageCode.Equals("en", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(localizedName))
            return "A localized unit name is required for the selected interface language.";
        if (localizedName?.Trim().Length > 200) return "The localized unit name is too long.";
        return null;
    }

    private static void SetTranslation(UnitOfMeasure unit, string languageCode, string? name)
    {
        if (languageCode.Equals("en", StringComparison.OrdinalIgnoreCase)) return;
        var translation = unit.Translations.SingleOrDefault(x => x.LanguageCode == languageCode);
        if (translation is null) unit.Translations.Add(new() { LanguageCode = languageCode, Name = name!.Trim() });
        else translation.Name = name!.Trim();
    }
    private static string NormalizeQuantityType(string value) => QuantityTypes.Single(x => x.Equals(value, StringComparison.OrdinalIgnoreCase));
    private bool SetVersion(UnitOfMeasure unit, string? version) { try { if (string.IsNullOrWhiteSpace(version)) return false; db.Entry(unit).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(version); return true; } catch (FormatException) { return false; } }
    private static string DisplayName(UnitOfMeasure x, string language) => x.Translations.FirstOrDefault(t => t.LanguageCode == language)?.Name ?? x.Name;
    private static UnitOfMeasureSummary Summary(UnitOfMeasure x, string language) => new(x.Id, x.Code, DisplayName(x, language), x.Name, x.Symbol, x.LetterCode, x.QuantityType, x.DecimalPlaces, x.IsActive);
    private static UnitOfMeasureDetails Details(UnitOfMeasure x, string language) => new(x.Id, x.Code, DisplayName(x, language), x.Name, x.Symbol, x.LetterCode, x.QuantityType, x.DecimalPlaces, x.IsActive, x.Translations.Select(t => new UnitTranslation(t.LanguageCode, t.Name)).ToArray(), Convert.ToBase64String(x.RowVersion));
    private static MasterDataResult<T> NotFound<T>() => MasterDataResult<T>.Failure(MasterDataError.NotFound, "Record was not found.");
    private static MasterDataResult<T> Validation<T>(string message) => MasterDataResult<T>.Failure(MasterDataError.Validation, message);
    private static MasterDataResult<T> Conflict<T>(string message) => MasterDataResult<T>.Failure(MasterDataError.Conflict, message);
}
