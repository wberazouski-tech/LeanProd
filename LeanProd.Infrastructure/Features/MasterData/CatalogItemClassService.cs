using LeanProd.Application.Features.MasterData;
using LeanProd.Domain.MasterData;
using LeanProd.Domain.MasterData.Policies;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.MasterData;

public sealed class CatalogItemClassService(LeanProdDbContext db) : ICatalogItemClassService
{
    public async Task<MasterDataPage<CatalogItemClassSummary>> GetCatalogItemClassesAsync(
        CatalogItemType type, MasterDataQuery query, bool? isGroup, CancellationToken ct)
    {
        var source = db.CatalogItemClasses.AsNoTracking().Where(x => x.Type == type);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
        }
        if (query.IsActive is not null) source = source.Where(x => x.IsActive == query.IsActive);
        if (isGroup is not null) source = source.Where(x => x.IsGroup == isGroup);
        var total = await source.CountAsync(ct);
        var items = await source.OrderBy(x => x.Code)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new CatalogItemClassSummary(
                x.Id, x.Type, x.Code, x.Name, x.IsGroup, x.ParentId, x.IsActive))
            .ToArrayAsync(ct);
        return new(items, query.Page, query.PageSize, total);
    }

    public async Task<CatalogItemClassDetails?> GetCatalogItemClassAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.CatalogItemClasses.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : Details(entity);
    }

    public async Task<IReadOnlyCollection<CatalogItemClassOption>> GetCatalogItemClassOptionsAsync(
        CatalogItemType type, bool activeOnly, CancellationToken ct)
    {
        var source = db.CatalogItemClasses.AsNoTracking().Where(x => x.Type == type);
        if (activeOnly) source = source.Where(x => x.IsActive);
        return await source.OrderBy(x => x.Code)
            .Select(x => new CatalogItemClassOption(x.Id, x.Type, x.Code, x.Name, x.IsGroup, x.ParentId))
            .ToArrayAsync(ct);
    }

    public async Task<MasterDataResult<CatalogItemClassDetails>> CreateCatalogItemClassAsync(
        SaveCatalogItemClassCommand command, CancellationToken ct)
    {
        command = command with
        {
            Code = await CodeGenerator.EnsureCodeAsync(command.Code,
                db.CatalogItemClasses.AsNoTracking().Where(x => x.Type == command.Type).Select(x => x.Code),
                4, ct)
        };
        var validation = await Validate(command, null, null, ct);
        if (validation is not null) return Validation(validation);
        var entity = new CatalogItemClass
        {
            Type = command.Type,
            Code = NormalizeCode(command.Code),
            Name = command.Name.Trim(),
            IsGroup = command.IsGroup,
            ParentId = command.ParentId
        };
        db.CatalogItemClasses.Add(entity);
        try
        {
            await db.SaveChangesAsync(ct);
            return MasterDataResult<CatalogItemClassDetails>.Success(Details(entity));
        }
        catch (DbUpdateException)
        {
            return Conflict("An item class with this code already exists in this item type.");
        }
    }

    public async Task<MasterDataResult<CatalogItemClassDetails>> UpdateCatalogItemClassAsync(
        Guid id, SaveCatalogItemClassCommand command, CancellationToken ct)
    {
        var entity = await db.CatalogItemClasses.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound<CatalogItemClassDetails>();
        command = command with
        {
            Code = await CodeGenerator.EnsureCodeAsync(command.Code,
                db.CatalogItemClasses.AsNoTracking().Where(x => x.Type == command.Type).Select(x => x.Code),
                4, ct)
        };
        var validation = await Validate(command, id, entity, ct);
        if (validation is not null) return Validation(validation);
        if (!SetVersion(entity, command.RowVersion)) return Validation("Row version is required.");
        entity.Type = command.Type;
        entity.Code = NormalizeCode(command.Code);
        entity.Name = command.Name.Trim();
        entity.IsGroup = command.IsGroup;
        entity.ParentId = command.ParentId;
        try
        {
            await db.SaveChangesAsync(ct);
            return MasterDataResult<CatalogItemClassDetails>.Success(Details(entity));
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("The item class was changed by another request.");
        }
        catch (DbUpdateException)
        {
            return Conflict("An item class with this code already exists in this item type.");
        }
    }

    public async Task<MasterDataResult<bool>> SetCatalogItemClassActiveAsync(
        Guid id, bool active, CancellationToken ct)
    {
        var entity = await db.CatalogItemClasses.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound<bool>();
        if (!active && await db.CatalogItemClasses.AnyAsync(x => x.ParentId == id && x.IsActive, ct))
            return MasterDataResult<bool>.Failure(MasterDataError.Dependency, "Deactivate child item classes first.");
        if (!active && await db.CatalogItems.AnyAsync(x => x.CatalogItemClassId == id && x.IsActive, ct))
            return MasterDataResult<bool>.Failure(MasterDataError.Dependency, "Deactivate catalog items in this class first.");
        entity.IsActive = active;
        await db.SaveChangesAsync(ct);
        return MasterDataResult<bool>.Success(true);
    }

    private async Task<string?> Validate(SaveCatalogItemClassCommand command, Guid? id,
        CatalogItemClass? current, CancellationToken ct)
    {
        var error = CatalogItemClassPolicy.ValidateInput(command.Type, command.Code, command.Name);
        if (error is not null) return error;

        if (current is not null && (current.IsGroup != command.IsGroup || current.Type != command.Type) &&
            await db.ItemPropertyDefinitions.AnyAsync(x => x.CatalogItemClassId == current.Id, ct))
            return "Cannot change the type or group flag of a class with property definitions.";

        if (current is not null)
        {
            var hasDependants = current.IsGroup != command.IsGroup &&
                (await db.CatalogItemClasses.AnyAsync(x => x.ParentId == current.Id, ct) ||
                 await db.CatalogItems.AnyAsync(x => x.CatalogItemClassId == current.Id, ct));
            error = CatalogItemClassPolicy.ValidateGroupFlagChange(
                current.IsGroup, command.IsGroup, hasDependants);
            if (error is not null) return error;
        }

        if (command.ParentId is not null)
        {
            var parent = await db.CatalogItemClasses.AsNoTracking()
                .Where(x => x.Id == command.ParentId)
                .Select(x => new CatalogItemClassParentFacts(x.Type, x.IsGroup, x.IsActive))
                .SingleOrDefaultAsync(ct);
            error = CatalogItemClassPolicy.ValidateParent(command.Type, parent);
            if (error is not null) return error;
        }

        var ancestors = await LoadAncestorIds(command.ParentId, ct);
        return HierarchyPolicy.Validate(id, command.ParentId, ancestors, "An item class");
    }

    private async Task<IReadOnlyCollection<Guid>> LoadAncestorIds(Guid? parentId, CancellationToken ct)
    {
        var ancestors = new List<Guid>();
        while (parentId is not null)
        {
            ancestors.Add(parentId.Value);
            parentId = await db.CatalogItemClasses.Where(x => x.Id == parentId)
                .Select(x => x.ParentId).SingleOrDefaultAsync(ct);
        }
        return ancestors;
    }

    private bool SetVersion(CatalogItemClass entity, string? version)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(version)) return false;
            db.Entry(entity).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(version);
            return true;
        }
        catch (FormatException) { return false; }
    }

    private static CatalogItemClassDetails Details(CatalogItemClass x) =>
        new(x.Id, x.Type, x.Code, x.Name, x.IsGroup, x.ParentId, x.IsActive,
            Convert.ToBase64String(x.RowVersion));
    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant();
    private static MasterDataResult<T> NotFound<T>() =>
        MasterDataResult<T>.Failure(MasterDataError.NotFound, "Record was not found.");
    private static MasterDataResult<CatalogItemClassDetails> Validation(string message) =>
        MasterDataResult<CatalogItemClassDetails>.Failure(MasterDataError.Validation, message);
    private static MasterDataResult<CatalogItemClassDetails> Conflict(string message) =>
        MasterDataResult<CatalogItemClassDetails>.Failure(MasterDataError.Conflict, message);
}
