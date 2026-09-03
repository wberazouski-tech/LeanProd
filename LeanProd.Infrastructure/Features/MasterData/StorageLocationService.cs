using LeanProd.Application.Features.MasterData;
using LeanProd.Domain.MasterData;
using LeanProd.Domain.MasterData.Policies;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.MasterData;

public sealed class StorageLocationService(LeanProdDbContext db) : IStorageLocationService
{
    public async Task<MasterDataPage<StorageLocationSummary>> GetStorageLocationsAsync(
        MasterDataQuery query, Guid? departmentId, Guid? kindId, Guid? typeId, CancellationToken ct)
    {
        var source = db.StorageLocations.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
        }
        if (query.IsActive is not null) source = source.Where(x => x.IsActive == query.IsActive);
        if (departmentId is not null) source = source.Where(x => x.DepartmentId == departmentId);
        if (kindId is not null) source = source.Where(x => x.KindId == kindId);
        if (typeId is not null)
            source = source.Where(x => x.TypeAssignments.Any(a => a.StorageLocationTypeId == typeId));
        var total = await source.CountAsync(ct);
        var page = await source.Include(x => x.Department).Include(x => x.Kind)
            .Include(x => x.TypeAssignments).ThenInclude(x => x.StorageLocationType)
            .OrderBy(x => x.Code).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToArrayAsync(ct);
        var items = page.Select(x => new StorageLocationSummary(
            x.Id, x.Code, x.Name, x.ParentStorageLocationId, x.Department.Name, x.Kind.Code,
            x.TypeAssignments.Select(a => a.StorageLocationType.Code).Order().ToArray(), x.IsActive))
            .ToArray();
        return new(items, query.Page, query.PageSize, total);
    }

    public async Task<StorageLocationDetails?> GetStorageLocationAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.StorageLocations.AsNoTracking().Include(x => x.TypeAssignments)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : Details(entity);
    }

    public async Task<IReadOnlyCollection<StorageLocationOption>> GetStorageLocationOptionsAsync(
        CancellationToken ct) =>
        await db.StorageLocations.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new StorageLocationOption(x.Id, x.Code, x.Name, x.DepartmentId))
            .ToArrayAsync(ct);

    public async Task<IReadOnlyCollection<LookupItem>> GetStorageLocationKindsAsync(CancellationToken ct) =>
        await db.StorageLocationKinds.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code)
            .Select(x => new LookupItem(x.Id, x.Code)).ToArrayAsync(ct);

    public async Task<IReadOnlyCollection<LookupItem>> GetStorageLocationTypesAsync(CancellationToken ct) =>
        await db.StorageLocationTypes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code)
            .Select(x => new LookupItem(x.Id, x.Code)).ToArrayAsync(ct);

    public async Task<MasterDataResult<StorageLocationDetails>> CreateStorageLocationAsync(
        SaveStorageLocationCommand command, CancellationToken ct)
    {
        command = command with
        {
            Code = await CodeGenerator.EnsureCodeAsync(command.Code,
                db.StorageLocations.AsNoTracking().Select(x => x.Code), 4, ct)
        };
        var validation = await Validate(command, null, ct);
        if (validation is not null) return Validation(validation);
        var entity = NewStorage(command);
        db.StorageLocations.Add(entity);
        try
        {
            await db.SaveChangesAsync(ct);
            return MasterDataResult<StorageLocationDetails>.Success(Details(entity));
        }
        catch (DbUpdateException)
        {
            return Conflict("A storage location with this code already exists.");
        }
    }

    public async Task<MasterDataResult<StorageLocationDetails>> UpdateStorageLocationAsync(
        Guid id, SaveStorageLocationCommand command, CancellationToken ct)
    {
        var entity = await db.StorageLocations.Include(x => x.TypeAssignments)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound<StorageLocationDetails>();
        command = command with
        {
            Code = await CodeGenerator.EnsureCodeAsync(command.Code,
                db.StorageLocations.AsNoTracking().Select(x => x.Code), 4, ct)
        };
        var validation = await Validate(command, id, ct);
        if (validation is not null) return Validation(validation);
        if (!SetVersion(entity, command.RowVersion)) return Validation("Row version is required.");
        entity.Code = NormalizeCode(command.Code);
        entity.Name = command.Name.Trim();
        entity.Description = Clean(command.Description);
        entity.DepartmentId = command.DepartmentId;
        entity.KindId = command.KindId;
        entity.ParentStorageLocationId = command.ParentStorageLocationId;
        SynchronizeTypes(entity, command.TypeIds);
        try
        {
            await db.SaveChangesAsync(ct);
            return MasterDataResult<StorageLocationDetails>.Success(Details(entity));
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("The storage location was changed by another request.");
        }
        catch (DbUpdateException)
        {
            return Conflict("A storage location with this code already exists.");
        }
    }

    public async Task<MasterDataResult<bool>> SetStorageLocationActiveAsync(
        Guid id, bool active, CancellationToken ct)
    {
        var entity = await db.StorageLocations.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound<bool>();
        if (!active && await db.StorageLocations.AnyAsync(
                x => x.ParentStorageLocationId == id && x.IsActive, ct))
            return MasterDataResult<bool>.Failure(MasterDataError.Dependency,
                "Deactivate child storage locations first.");
        entity.IsActive = active;
        await db.SaveChangesAsync(ct);
        return MasterDataResult<bool>.Success(true);
    }

    private async Task<string?> Validate(SaveStorageLocationCommand command, Guid? id, CancellationToken ct)
    {
        var error = StorageLocationPolicy.ValidateInput(command.Code, command.Name, command.TypeIds);
        if (error is not null) return error;

        var typeIds = command.TypeIds.Distinct().ToArray();
        var departmentIsActive = await db.Departments.AnyAsync(
            x => x.Id == command.DepartmentId && x.IsActive, ct);
        var kindIsActive = await db.StorageLocationKinds.AnyAsync(
            x => x.Id == command.KindId && x.IsActive, ct);
        var activeTypeCount = await db.StorageLocationTypes.CountAsync(
            x => typeIds.Contains(x.Id) && x.IsActive, ct);
        error = StorageLocationPolicy.ValidateReferences(
            departmentIsActive, kindIsActive, typeIds.Length, activeTypeCount);
        if (error is not null) return error;

        if (command.ParentStorageLocationId is not null &&
            !await db.StorageLocations.AnyAsync(
                x => x.Id == command.ParentStorageLocationId && x.IsActive, ct))
            return "The parent storage location must be active.";
        var ancestors = await LoadAncestorIds(command.ParentStorageLocationId, ct);
        return HierarchyPolicy.Validate(id, command.ParentStorageLocationId, ancestors,
            "A storage location");
    }

    private async Task<IReadOnlyCollection<Guid>> LoadAncestorIds(Guid? parentId, CancellationToken ct)
    {
        var ancestors = new List<Guid>();
        while (parentId is not null)
        {
            ancestors.Add(parentId.Value);
            parentId = await db.StorageLocations.Where(x => x.Id == parentId)
                .Select(x => x.ParentStorageLocationId).SingleOrDefaultAsync(ct);
        }
        return ancestors;
    }

    private void SynchronizeTypes(StorageLocation entity, IReadOnlyCollection<Guid> requestedTypeIds)
    {
        var desiredTypes = requestedTypeIds.Distinct().ToHashSet();
        foreach (var assignment in entity.TypeAssignments
                     .Where(x => !desiredTypes.Contains(x.StorageLocationTypeId)).ToArray())
            db.Remove(assignment);
        var existingTypes = entity.TypeAssignments.Select(x => x.StorageLocationTypeId).ToHashSet();
        foreach (var typeId in desiredTypes.Where(x => !existingTypes.Contains(x)))
            entity.TypeAssignments.Add(new StorageLocationTypeAssignment { StorageLocationTypeId = typeId });
    }

    private static StorageLocation NewStorage(SaveStorageLocationCommand command)
    {
        var entity = new StorageLocation
        {
            Code = NormalizeCode(command.Code),
            Name = command.Name.Trim(),
            Description = Clean(command.Description),
            DepartmentId = command.DepartmentId,
            KindId = command.KindId,
            ParentStorageLocationId = command.ParentStorageLocationId
        };
        foreach (var typeId in command.TypeIds.Distinct())
            entity.TypeAssignments.Add(new StorageLocationTypeAssignment { StorageLocationTypeId = typeId });
        return entity;
    }

    private bool SetVersion(StorageLocation entity, string? version)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(version)) return false;
            db.Entry(entity).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(version);
            return true;
        }
        catch (FormatException) { return false; }
    }

    private static StorageLocationDetails Details(StorageLocation x) =>
        new(x.Id, x.Code, x.Name, x.Description, x.DepartmentId, x.KindId,
            x.ParentStorageLocationId, x.TypeAssignments.Select(a => a.StorageLocationTypeId).ToArray(),
            x.IsActive, Convert.ToBase64String(x.RowVersion));
    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static MasterDataResult<T> NotFound<T>() =>
        MasterDataResult<T>.Failure(MasterDataError.NotFound, "Record was not found.");
    private static MasterDataResult<StorageLocationDetails> Validation(string message) =>
        MasterDataResult<StorageLocationDetails>.Failure(MasterDataError.Validation, message);
    private static MasterDataResult<StorageLocationDetails> Conflict(string message) =>
        MasterDataResult<StorageLocationDetails>.Failure(MasterDataError.Conflict, message);
}
