using System.Data;
using LeanProd.Application.Features.MasterData;
using LeanProd.Domain.Common;
using LeanProd.Domain.MasterData;
using LeanProd.Domain.MasterData.Policies;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.MasterData;

public sealed class ItemPropertyService(LeanProdDbContext db) : IItemPropertyService
{
    public async Task<MasterDataResult<IReadOnlyList<PropertyDefinitionDto>>> Definitions(Guid classId, CancellationToken ct)
    {
        if (!await db.CatalogItemClasses.AnyAsync(x => x.Id == classId, ct)) return Missing<IReadOnlyList<PropertyDefinitionDto>>();
        var used = await UsedPropertyIds(classId, ct);
        return Ok<IReadOnlyList<PropertyDefinitionDto>>((await LoadDefinitions(classId, ct)).Select(p => Definition(p, used.Contains(p.Id))).ToArray());
    }

    public Task<MasterDataResult<PropertyDefinitionDto>> SaveDefinition(Guid classId, Guid? id, SavePropertyCommand c, CancellationToken ct) => Write(async () =>
    {
        var itemClass = await db.CatalogItemClasses.SingleOrDefaultAsync(x => x.Id == classId, ct);
        if (itemClass is null) return Missing<PropertyDefinitionDto>();
        if (itemClass.IsGroup || !itemClass.IsActive) return Invalid<PropertyDefinitionDto>("Choose an active leaf class.");
        if (!id.HasValue && await db.ItemPropertyDefinitions.CountAsync(x => x.CatalogItemClassId == classId, ct) >= 500)
            return Invalid<PropertyDefinitionDto>("A class supports at most 500 property definitions.");
        var p = id.HasValue ? await db.ItemPropertyDefinitions.Include(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == id && x.CatalogItemClassId == classId, ct) : new ItemPropertyDefinition { CatalogItemClassId = classId };
        if (p is null) return Missing<PropertyDefinitionDto>();
        if (c.Options is null || c.Options.Any(x => x is null)) return Invalid<PropertyDefinitionDto>("Choices are required (use an empty list for other types).");
        var candidate = new ItemPropertyDefinition
        {
            Name = c.Name?.Trim() ?? "", Type = c.Type,
            DecimalPlaces = c.DecimalPlaces, MaxLength = c.MaxLength, Minimum = c.Minimum, Maximum = c.Maximum,
            IsBatchProperty = c.IsBatchProperty,
            Options = c.Options.Select(x => new ItemPropertyOption { Id = x.Id ?? Guid.NewGuid(), Label = x.Label?.Trim() ?? "" }).ToArray()
        };
        var error = ItemPropertyPolicy.ValidateDefinition(candidate);
        if (error is not null) return Invalid<PropertyDefinitionDto>(error);
        if (candidate.Options.Select(x => x.Id).Distinct().Count() != candidate.Options.Count)
            return Invalid<PropertyDefinitionDto>("Duplicate choice identifiers.");
        if (c.Options.Any(x => x.Id.HasValue && !p.Options.Any(o => o.Id == x.Id)))
            return Invalid<PropertyDefinitionDto>("Unknown choice identifier.");
        var used = false;
        if (id.HasValue)
        {
            if (!Version(p, c.RowVersion)) return Invalid<PropertyDefinitionDto>("A valid row version is required.");
            used = await db.CatalogItemPropertyValues.AnyAsync(x => x.PropertyId == id, ct) ||
                await db.BatchPropertyValues.AnyAsync(x => x.PropertyId == id, ct);
            if (used && (p.Type != c.Type || p.DecimalPlaces != c.DecimalPlaces || p.MaxLength != c.MaxLength ||
                p.Minimum != c.Minimum || p.Maximum != c.Maximum || p.IsBatchProperty != c.IsBatchProperty ||
                p.Options.Any(o => !candidate.Options.Any(n => n.Id == o.Id && n.Label == o.Label))))
                return Conflict<PropertyDefinitionDto>("This property is in use. Its type, settings and existing choices cannot change; create a new property instead.");
        }
        else db.ItemPropertyDefinitions.Add(p);
        p.Name = candidate.Name; p.Type = c.Type;
        p.DecimalPlaces = c.DecimalPlaces; p.MaxLength = c.MaxLength; p.Minimum = c.Minimum; p.Maximum = c.Maximum;
        p.IsBatchProperty = c.IsBatchProperty; p.IsActive = c.IsActive;
        foreach (var old in p.Options.ToArray())
        {
            var next = candidate.Options.SingleOrDefault(x => x.Id == old.Id);
            if (next is null) { p.Options.Remove(old); db.ItemPropertyOptions.Remove(old); }
            else old.Label = next.Label;
        }
        foreach (var next in candidate.Options.Where(x => p.Options.All(o => o.Id != x.Id)))
        {
            next.PropertyId = p.Id;
            p.Options.Add(next);
            db.ItemPropertyOptions.Add(next);
        }
        if (id.HasValue) Touch(p);
        Touch(itemClass);
        await db.SaveChangesAsync(ct);
        return Ok(Definition(p, used));
    }, ct);

    public async Task<MasterDataResult<PropertyValuesDto>> Values(Guid itemId, Guid? batchId, CancellationToken ct)
    {
        var item = await db.CatalogItems.AsNoTracking().SingleOrDefaultAsync(x => x.Id == itemId, ct);
        if (item is null) return Missing<PropertyValuesDto>();
        var definitions = await LoadDefinitions(item.CatalogItemClassId, ct);
        var used = await UsedPropertyIds(item.CatalogItemClassId, ct);
        if (batchId.HasValue)
        {
            var batch = await db.CatalogItemBatches.AsNoTracking().Include(x => x.Values)
                .SingleOrDefaultAsync(x => x.Id == batchId && x.CatalogItemId == itemId, ct);
            if (batch is null) return Missing<PropertyValuesDto>();
            return Ok(new PropertyValuesDto(Convert.ToBase64String(batch.RowVersion),
                definitions.Where(x => x.IsBatchProperty).Select(p => Definition(p, used.Contains(p.Id))).ToArray(),
                batch.Values.Select(x => new PropertyValueDto(x.PropertyId, x.Number, x.Upper, x.Text, x.Boolean, x.OptionId)).ToArray(), Batch(batch), item.IsActive));
        }
        var values = await db.CatalogItemPropertyValues.AsNoTracking().Where(x => x.CatalogItemId == itemId)
            .Select(x => new PropertyValueDto(x.PropertyId, x.Number, x.Upper, x.Text, x.Boolean, x.OptionId)).ToArrayAsync(ct);
        return Ok(new PropertyValuesDto(Convert.ToBase64String(item.RowVersion), definitions.Select(p => Definition(p, used.Contains(p.Id))).ToArray(), values, IsEditable: item.IsActive));
    }

    public Task<MasterDataResult<PropertyValuesDto>> SaveValues(Guid itemId, Guid? batchId, SavePropertyValuesCommand c, CancellationToken ct) => Write(async () =>
    {
        var item = await db.CatalogItems.SingleOrDefaultAsync(x => x.Id == itemId, ct);
        if (item is null) return Missing<PropertyValuesDto>();
        if (!item.IsActive) return Invalid<PropertyValuesDto>("The catalog item is inactive.");
        CatalogItemBatch? batch = null;
        if (batchId.HasValue)
        {
            batch = await db.CatalogItemBatches.Include(x => x.Values).SingleOrDefaultAsync(x => x.Id == batchId && x.CatalogItemId == itemId, ct);
            if (batch is null) return Missing<PropertyValuesDto>();
        }
        AuditableEntity owner = batch is null ? item : batch;
        if (!Version(owner, c.RowVersion)) return Invalid<PropertyValuesDto>("A valid row version is required.");
        if (c.Values is null || c.Values.Count > 500 || c.Values.Any(x => x is null) || c.Values.Select(x => x.PropertyId).Distinct().Count() != c.Values.Count)
            return Invalid<PropertyValuesDto>("Provide at most 500 values with unique property identifiers.");
        var definitions = await LoadDefinitions(item.CatalogItemClassId, ct);
        foreach (var v in c.Values)
        {
            var p = definitions.SingleOrDefault(x => x.Id == v.PropertyId);
            if (p is null || !p.IsActive || batch is not null && !p.IsBatchProperty)
                return Invalid<PropertyValuesDto>("Property is inactive or does not belong to this class or batch.");
            var error = ItemPropertyPolicy.ValidateValue(p, v.Number, v.Upper, v.Text, v.Boolean, v.OptionId);
            if (error is not null) return Invalid<PropertyValuesDto>($"{p.Name}: {error}");
        }
        // Only submitted properties change. An explicit empty value clears an active property.
        if (batch is null)
        {
            var existing = await db.CatalogItemPropertyValues.Where(x => x.CatalogItemId == itemId).ToListAsync(ct);
            foreach (var v in c.Values)
            {
                var value = existing.SingleOrDefault(x => x.PropertyId == v.PropertyId);
                if (Empty(v)) { if (value is not null) db.CatalogItemPropertyValues.Remove(value); continue; }
                if (value is null) { value = new() { CatalogItemId = itemId, PropertyId = v.PropertyId }; db.CatalogItemPropertyValues.Add(value); }
                value.Number = v.Number; value.Upper = v.Upper; value.Text = v.Text; value.Boolean = v.Boolean; value.OptionId = v.OptionId;
            }
        }
        else
        {
            foreach (var v in c.Values)
            {
                var value = batch.Values.SingleOrDefault(x => x.PropertyId == v.PropertyId);
                if (Empty(v)) { if (value is not null) db.BatchPropertyValues.Remove(value); continue; }
                if (value is null) { value = new() { BatchId = batch.Id, PropertyId = v.PropertyId }; db.BatchPropertyValues.Add(value); }
                value.Number = v.Number; value.Upper = v.Upper; value.Text = v.Text; value.Boolean = v.Boolean; value.OptionId = v.OptionId;
            }
        }
        Touch(owner);
        await db.SaveChangesAsync(ct);
        return await Values(itemId, batchId, ct);
    }, ct);

    public async Task<MasterDataResult<MasterDataPage<BatchDto>>> Batches(Guid itemId, int page, CancellationToken ct)
    {
        if (!await db.CatalogItems.AnyAsync(x => x.Id == itemId, ct)) return Missing<MasterDataPage<BatchDto>>();
        if (page < 1 || page > 1000000) return Invalid<MasterDataPage<BatchDto>>("Invalid page.");
        var query = db.CatalogItemBatches.AsNoTracking().Where(x => x.CatalogItemId == itemId);
        var count = await query.CountAsync(ct);
        var batches = await query.OrderByDescending(x => x.ReceiptDate).ThenBy(x => x.Id).Skip((page - 1) * 50).Take(50).ToArrayAsync(ct);
        return Ok(new MasterDataPage<BatchDto>(batches.Select(Batch).ToArray(), page, 50, count));
    }

    public Task<MasterDataResult<BatchDto>> SaveBatch(Guid itemId, Guid? id, SaveBatchCommand c, CancellationToken ct) => Write(async () =>
    {
        var item = await db.CatalogItems.SingleOrDefaultAsync(x => x.Id == itemId, ct);
        if (item is null) return Missing<BatchDto>();
        if (!item.IsActive) return Invalid<BatchDto>("The catalog item is inactive.");
        if (string.IsNullOrWhiteSpace(c.Number) || c.Number.Trim().Length > 100 ||
            string.IsNullOrWhiteSpace(c.ReceiptReference) || c.ReceiptReference.Trim().Length > 300 || c.ReceiptDate == default)
            return Invalid<BatchDto>("Batch number (up to 100), receipt date and document reference (up to 300) are required.");
        var batch = id.HasValue ? await db.CatalogItemBatches.SingleOrDefaultAsync(x => x.Id == id && x.CatalogItemId == itemId, ct)
            : new CatalogItemBatch { CatalogItemId = itemId };
        if (batch is null) return Missing<BatchDto>();
        if (id.HasValue && !Version(batch, c.RowVersion)) return Invalid<BatchDto>("A valid row version is required.");
        batch.Number = c.Number.Trim().ToUpperInvariant(); batch.ReceiptDate = c.ReceiptDate; batch.ReceiptReference = c.ReceiptReference.Trim();
        if (!id.HasValue)
        {
            db.CatalogItemBatches.Add(batch);
            var defaults = await (from v in db.CatalogItemPropertyValues
                join p in db.ItemPropertyDefinitions on v.PropertyId equals p.Id
                where v.CatalogItemId == itemId && p.CatalogItemClassId == item.CatalogItemClassId && p.IsActive && p.IsBatchProperty
                select v).ToArrayAsync(ct);
            foreach (var v in defaults) batch.Values.Add(new BatchPropertyValue
            { BatchId = batch.Id, PropertyId = v.PropertyId, Number = v.Number, Upper = v.Upper, Text = v.Text, Boolean = v.Boolean, OptionId = v.OptionId });
            // Competes with class changes using the same item row version.
            Touch(item);
        }
        else Touch(batch);
        await db.SaveChangesAsync(ct);
        return Ok(Batch(batch));
    }, ct);

    private async Task<HashSet<Guid>> UsedPropertyIds(Guid classId, CancellationToken ct)
    {
        var definitions = db.ItemPropertyDefinitions.Where(p => p.CatalogItemClassId == classId).Select(p => p.Id);
        var ids = await db.CatalogItemPropertyValues.Where(v => definitions.Contains(v.PropertyId)).Select(v => v.PropertyId)
            .Union(db.BatchPropertyValues.Where(v => definitions.Contains(v.PropertyId)).Select(v => v.PropertyId))
            .ToArrayAsync(ct);
        return ids.ToHashSet();
    }

    private Task<List<ItemPropertyDefinition>> LoadDefinitions(Guid classId, CancellationToken ct) =>
        db.ItemPropertyDefinitions.AsNoTracking().Include(x => x.Options).Where(x => x.CatalogItemClassId == classId).OrderBy(x => x.Name).ThenBy(x => x.Id).ToListAsync(ct);
    private bool Version(AuditableEntity entity, string? version)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(version)) return false;
            var bytes = Convert.FromBase64String(version);
            if (bytes.Length != 8) return false;
            db.Entry(entity).Property(x => x.RowVersion).OriginalValue = bytes;
            return true;
        }
        catch (FormatException) { return false; }
    }
    private void Touch(AuditableEntity entity) => db.Entry(entity).Property(x => x.UpdatedAtUtc).IsModified = true;
    private async Task<MasterDataResult<T>> Write<T>(Func<Task<MasterDataResult<T>>> action, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            var result = await action();
            if (result.Succeeded) await transaction.CommitAsync(ct);
            return result;
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 1205 or 1222) { return Conflict<T>("Concurrent change detected. Reload and try again."); }
        catch (DbUpdateConcurrencyException) { return Conflict<T>("The record changed. Reload and try again."); }
        catch (DbUpdateException) { return Conflict<T>("The change conflicts with existing data. Check duplicate choices or batch numbers and reload."); }
    }
    private static bool Empty(PropertyValueDto v) => v.Number is null && v.Upper is null && v.Text is null && v.Boolean is null && v.OptionId is null;
    private static PropertyDefinitionDto Definition(ItemPropertyDefinition p, bool used = false) => new(p.Id, p.CatalogItemClassId, p.Name, p.Type,
        p.DecimalPlaces, p.MaxLength, p.Minimum, p.Maximum, p.IsBatchProperty, p.IsActive,
        p.Options.OrderBy(x => x.Label).Select(x => new PropertyOptionDto(x.Id, x.Label)).ToArray(), Convert.ToBase64String(p.RowVersion), used);
    private static BatchDto Batch(CatalogItemBatch b) => new(b.Id, b.CatalogItemId, b.Number, b.ReceiptDate, b.ReceiptReference, Convert.ToBase64String(b.RowVersion));
    private static MasterDataResult<T> Ok<T>(T value) => MasterDataResult<T>.Success(value);
    private static MasterDataResult<T> Missing<T>() => MasterDataResult<T>.Failure(MasterDataError.NotFound, "Record was not found.");
    private static MasterDataResult<T> Invalid<T>(string message) => MasterDataResult<T>.Failure(MasterDataError.Validation, message);
    private static MasterDataResult<T> Conflict<T>(string message) => MasterDataResult<T>.Failure(MasterDataError.Conflict, message);
}
