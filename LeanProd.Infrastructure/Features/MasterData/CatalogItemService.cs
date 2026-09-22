using LeanProd.Application.Features.MasterData;
using LeanProd.Domain.Common;
using LeanProd.Domain.MasterData;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.MasterData;

public sealed class CatalogItemService(LeanProdDbContext db, TimeProvider timeProvider) : ICatalogItemService
{
    public async Task<MasterDataPage<CatalogItemSummary>> GetItemsAsync(
        CatalogItemType type, MasterDataQuery query, CancellationToken ct)
    {
        var source = db.CatalogItems.AsNoTracking().Where(x => x.Type == type);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.WorkingName.Contains(search)
                || (x.FullName != null && x.FullName.Contains(search))
                || (x.ArticleNumber != null && x.ArticleNumber.Contains(search)));
        }

        if (query.IsActive is not null)
            source = source.Where(x => x.IsActive == query.IsActive);

        var total = await source.CountAsync(ct);
        var items = await source.OrderBy(x => x.WorkingName)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new CatalogItemSummary(x.Id, x.WorkingName, x.FullName,
                x.ArticleNumber, x.Type, x.BaseUnitOfMeasureId,
                x.BaseUnitOfMeasure.Name, x.BaseUnitOfMeasure.Symbol,
                x.CatalogItemClassId, x.CatalogItemClass.Code, x.CatalogItemClass.Name,
                x.CostHistory.OrderByDescending(cost => cost.EffectiveFromUtc)
                    .ThenByDescending(cost => cost.Id).Select(cost => cost.Amount).FirstOrDefault(),
                x.IsActive))
            .ToArrayAsync(ct);
        return new(items, query.Page, query.PageSize, total);
    }

    public async Task<CatalogItemDetails?> GetItemAsync(Guid id, CancellationToken ct)
    {
        var item = await db.CatalogItems.AsNoTracking()
            .Include(x => x.BaseUnitOfMeasure)
            .Include(x => x.CatalogItemClass)
            .Include(x => x.CostHistory)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        return item is null ? null : Details(item);
    }

    public async Task<MasterDataResult<CatalogItemDetails>> CreateItemAsync(
        SaveCatalogItemCommand command, CancellationToken ct)
    {
        var validation = await Validate(command, null, ct);
        if (validation is not null) return validation;

        var item = new LeanProd.Domain.MasterData.CatalogItem();
        Apply(item, command);
        db.CatalogItems.Add(item);
        AddCost(item, command.Cost);
        try
        {
            await db.SaveChangesAsync(ct);
            return MasterDataResult<CatalogItemDetails>.Success((await GetItemAsync(item.Id, ct))!);
        }
        catch (DbUpdateException)
        {
            return Conflict("An item with this article number already exists in this item type.");
        }
    }

    public async Task<MasterDataResult<CatalogItemDetails>> UpdateItemAsync(
        Guid id, SaveCatalogItemCommand command, CancellationToken ct)
    {
        var item = await db.CatalogItems.Include(x => x.BaseUnitOfMeasure).Include(x => x.CostHistory)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
            return MasterDataResult<CatalogItemDetails>.Failure(MasterDataError.NotFound, "Record was not found.");

        var validation = await Validate(command, id, ct);
        if (validation is not null) return validation;
        if (!SetVersion(item, command.RowVersion))
            return Validation("Row version is required.");

        Apply(item, command);
        var currentCost = item.CostHistory.OrderByDescending(x => x.EffectiveFromUtc)
            .ThenByDescending(x => x.Id).FirstOrDefault()?.Amount;
        if (currentCost != command.Cost)
        {
            AddCost(item, command.Cost);
            db.Entry(item).Property(x => x.Description).IsModified = true;
        }
        try
        {
            await db.SaveChangesAsync(ct);
            return MasterDataResult<CatalogItemDetails>.Success((await GetItemAsync(item.Id, ct))!);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("The item was changed by another request.");
        }
        catch (DbUpdateException)
        {
            return Conflict("An item with this article number already exists in this item type.");
        }
    }

    public async Task<MasterDataResult<CatalogItemDetails>> ChangeItemClassAsync(
        Guid id, Guid catalogItemClassId, CancellationToken ct)
    {
        var item = await db.CatalogItems.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
            return MasterDataResult<CatalogItemDetails>.Failure(MasterDataError.NotFound, "Record was not found.");

        var validation = await ValidateItemClass(catalogItemClassId, item.Type, ct);
        if (validation is not null) return validation;

        if (item.CatalogItemClassId != catalogItemClassId && await HasPropertyData(item.Id, ct))
            return Conflict("Cannot change class while the item has property values or batches.");
        item.CatalogItemClassId = catalogItemClassId;
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return Conflict("The item was changed by another request."); }
        return MasterDataResult<CatalogItemDetails>.Success((await GetItemAsync(item.Id, ct))!);
    }

    public async Task<MasterDataResult<bool>> SetItemActiveAsync(Guid id, bool active, CancellationToken ct)
    {
        var item = await db.CatalogItems.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
            return MasterDataResult<bool>.Failure(MasterDataError.NotFound, "Record was not found.");
        item.IsActive = active;
        await db.SaveChangesAsync(ct);
        return MasterDataResult<bool>.Success(true);
    }

    private async Task<MasterDataResult<CatalogItemDetails>?> Validate(
        SaveCatalogItemCommand command, Guid? id, CancellationToken ct)
    {
        if (!Enum.IsDefined(command.Type)) return Validation("Item type is invalid.");
        if (command.Cost < 0) return Validation("Cost cannot be negative.");
        if (decimal.Round(command.Cost, 2) != command.Cost)
            return Validation("Cost cannot have more than 2 decimal places.");
        if (command.Cost > 9999999999999999.99m) return Validation("Cost is too large.");
        if (string.IsNullOrWhiteSpace(command.WorkingName)) return Validation("Working name is required.");
        if (command.WorkingName.Trim().Length > 200) return Validation("Working name cannot exceed 200 characters.");

        var unitIsActive = await db.UnitOfMeasures.AnyAsync(
            x => x.Id == command.BaseUnitOfMeasureId && x.IsActive, ct);
        if (!unitIsActive) return Validation("An active base unit of measure is required.");

        var itemClassValidation = await ValidateItemClass(command.CatalogItemClassId, command.Type, ct);
        if (itemClassValidation is not null) return itemClassValidation;

        if (id.HasValue && await db.CatalogItems.AnyAsync(x => x.Id == id && x.CatalogItemClassId != command.CatalogItemClassId, ct)
            && await HasPropertyData(id.Value, ct))
            return Conflict("Cannot change class while the item has property values or batches.");
        var article = NormalizeArticle(command.ArticleNumber);
        if (article is not null && await db.CatalogItems.AnyAsync(
                x => x.Id != id && x.Type == command.Type && x.ArticleNumber == article, ct))
            return Conflict("An item with this article number already exists in this item type.");
        return null;
    }

    private async Task<MasterDataResult<CatalogItemDetails>?> ValidateItemClass(
        Guid catalogItemClassId, CatalogItemType type, CancellationToken ct)
    {
        var itemClass = await db.CatalogItemClasses.AsNoTracking()
            .Where(x => x.Id == catalogItemClassId)
            .Select(x => new { x.Type, x.IsGroup, x.IsActive })
            .SingleOrDefaultAsync(ct);
        if (itemClass is null || !itemClass.IsActive)
            return Validation("An active item class is required.");
        if (itemClass.IsGroup)
            return Validation("Catalog items cannot be assigned to an item class group.");
        if (itemClass.Type != type)
            return Validation("Item class must belong to the selected item type.");
        return null;
    }

    private async Task<bool> HasPropertyData(Guid id, CancellationToken ct) =>
        await db.CatalogItemPropertyValues.AnyAsync(x => x.CatalogItemId == id, ct) ||
        await db.CatalogItemBatches.AnyAsync(x => x.CatalogItemId == id, ct);

    private static void Apply(LeanProd.Domain.MasterData.CatalogItem item, SaveCatalogItemCommand command)
    {
        item.WorkingName = command.WorkingName.Trim();
        item.FullName = Clean(command.FullName);
        item.ArticleNumber = NormalizeArticle(command.ArticleNumber);
        item.Type = command.Type;
        item.CatalogItemClassId = command.CatalogItemClassId;
        item.BaseUnitOfMeasureId = command.BaseUnitOfMeasureId;
        item.Description = Clean(command.Description);
    }

    private bool SetVersion(AuditableEntity entity, string? version)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(version)) return false;
            db.Entry(entity).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(version);
            return true;
        }
        catch (FormatException) { return false; }
    }

    private void AddCost(LeanProd.Domain.MasterData.CatalogItem item, decimal amount)
    {
        var cost = new CatalogItemCostHistory
        {
            CatalogItemId = item.Id,
            Amount = amount,
            EffectiveFromUtc = timeProvider.GetUtcNow().UtcDateTime
        };
        item.CostHistory.Add(cost);
        db.CatalogItemCostHistory.Add(cost);
    }

    private static CatalogItemDetails Details(LeanProd.Domain.MasterData.CatalogItem x) => new(x.Id, x.WorkingName,
        x.FullName, x.ArticleNumber, x.Type, x.BaseUnitOfMeasureId,
        x.BaseUnitOfMeasure.Name, x.BaseUnitOfMeasure.Symbol,
        x.CatalogItemClassId, x.CatalogItemClass.Code, x.CatalogItemClass.Name,
        x.CostHistory.OrderByDescending(cost => cost.EffectiveFromUtc)
            .ThenByDescending(cost => cost.Id).FirstOrDefault()?.Amount ?? 0m, x.Description,
        x.IsActive, Convert.ToBase64String(x.RowVersion));
    private static string? NormalizeArticle(string? value) => Clean(value)?.ToUpperInvariant();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static MasterDataResult<CatalogItemDetails> Validation(string message) =>
        MasterDataResult<CatalogItemDetails>.Failure(MasterDataError.Validation, message);
    private static MasterDataResult<CatalogItemDetails> Conflict(string message) =>
        MasterDataResult<CatalogItemDetails>.Failure(MasterDataError.Conflict, message);
}
