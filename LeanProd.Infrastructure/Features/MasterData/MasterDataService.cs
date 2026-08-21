using LeanProd.Application.Features.MasterData;
using LeanProd.Domain.MasterData;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.MasterData;

public sealed class MasterDataService(LeanProdDbContext db) : IMasterDataService
{
    public async Task<MasterDataPage<DepartmentSummary>> GetDepartmentsAsync(MasterDataQuery query, CancellationToken ct)
    {
        var source = db.Departments.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search)) { var s = query.Search.Trim(); source = source.Where(x => x.Code.Contains(s) || x.Name.Contains(s)); }
        if (query.IsActive is not null) source = source.Where(x => x.IsActive == query.IsActive);
        var total = await source.CountAsync(ct);
        var items = await source.OrderBy(x => x.Code).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new DepartmentSummary(x.Id, x.Code, x.Name, x.ParentDepartmentId, x.ParentDepartment == null ? null : x.ParentDepartment.Name, x.IsActive)).ToArrayAsync(ct);
        return new(items, query.Page, query.PageSize, total);
    }

    public async Task<DepartmentDetails?> GetDepartmentAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.Departments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : DepartmentDetails(entity);
    }

    public async Task<IReadOnlyCollection<OptionItem>> GetDepartmentOptionsAsync(CancellationToken ct) =>
        await db.Departments.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new OptionItem(x.Id, x.Code, x.Name)).ToArrayAsync(ct);

    public async Task<MasterDataResult<DepartmentDetails>> CreateDepartmentAsync(SaveDepartmentCommand command, CancellationToken ct)
    {
        var validation = await ValidateDepartment(command, null, ct); if (validation is not null) return validation;
        var entity = new Department { Code = NormalizeCode(command.Code), Name = command.Name.Trim(), Description = Clean(command.Description), ParentDepartmentId = command.ParentDepartmentId };
        db.Departments.Add(entity);
        try { await db.SaveChangesAsync(ct); return MasterDataResult<DepartmentDetails>.Success(DepartmentDetails(entity)); }
        catch (DbUpdateException) { return Conflict<DepartmentDetails>("A department with this code already exists."); }
    }

    public async Task<MasterDataResult<DepartmentDetails>> UpdateDepartmentAsync(Guid id, SaveDepartmentCommand command, CancellationToken ct)
    {
        var entity = await db.Departments.SingleOrDefaultAsync(x => x.Id == id, ct); if (entity is null) return NotFound<DepartmentDetails>();
        var validation = await ValidateDepartment(command, id, ct); if (validation is not null) return validation;
        if (!SetVersion(entity, command.RowVersion)) return Validation<DepartmentDetails>("Row version is required.");
        entity.Code = NormalizeCode(command.Code); entity.Name = command.Name.Trim(); entity.Description = Clean(command.Description); entity.ParentDepartmentId = command.ParentDepartmentId;
        try { await db.SaveChangesAsync(ct); return MasterDataResult<DepartmentDetails>.Success(DepartmentDetails(entity)); }
        catch (DbUpdateConcurrencyException) { return Conflict<DepartmentDetails>("The department was changed by another request."); }
        catch (DbUpdateException) { return Conflict<DepartmentDetails>("A department with this code already exists."); }
    }

    public async Task<MasterDataResult<bool>> SetDepartmentActiveAsync(Guid id, bool active, CancellationToken ct)
    {
        var entity = await db.Departments.SingleOrDefaultAsync(x => x.Id == id, ct); if (entity is null) return NotFound<bool>();
        if (!active && (await db.Departments.AnyAsync(x => x.ParentDepartmentId == id && x.IsActive, ct) || await db.StorageLocations.AnyAsync(x => x.DepartmentId == id && x.IsActive, ct)))
            return MasterDataResult<bool>.Failure(MasterDataError.Dependency, "Deactivate child departments and storage locations first.");
        entity.IsActive = active; await db.SaveChangesAsync(ct); return MasterDataResult<bool>.Success(true);
    }

    public async Task<MasterDataPage<StorageLocationSummary>> GetStorageLocationsAsync(MasterDataQuery query, Guid? departmentId, Guid? kindId, Guid? typeId, CancellationToken ct)
    {
        var source = db.StorageLocations.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search)) { var s = query.Search.Trim(); source = source.Where(x => x.Code.Contains(s) || x.Name.Contains(s)); }
        if (query.IsActive is not null) source = source.Where(x => x.IsActive == query.IsActive);
        if (departmentId is not null) source = source.Where(x => x.DepartmentId == departmentId);
        if (kindId is not null) source = source.Where(x => x.KindId == kindId);
        if (typeId is not null) source = source.Where(x => x.TypeAssignments.Any(a => a.StorageLocationTypeId == typeId));
        var total = await source.CountAsync(ct);
        var page = await source.Include(x => x.Department).Include(x => x.Kind).Include(x => x.TypeAssignments).ThenInclude(x => x.StorageLocationType)
            .OrderBy(x => x.Code).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToArrayAsync(ct);
        var items = page.Select(x => new StorageLocationSummary(x.Id, x.Code, x.Name, x.ParentStorageLocationId, x.Department.Name, x.Kind.Code,
            x.TypeAssignments.Select(a => a.StorageLocationType.Code).Order().ToArray(), x.IsActive)).ToArray();
        return new(items, query.Page, query.PageSize, total);
    }

    public async Task<StorageLocationDetails?> GetStorageLocationAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.StorageLocations.AsNoTracking().Include(x => x.TypeAssignments).SingleOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : StorageDetails(entity);
    }

    public async Task<IReadOnlyCollection<StorageLocationOption>> GetStorageLocationOptionsAsync(CancellationToken ct) =>
        await db.StorageLocations.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new StorageLocationOption(x.Id, x.Code, x.Name, x.DepartmentId)).ToArrayAsync(ct);
    public async Task<IReadOnlyCollection<CatalogItem>> GetStorageLocationKindsAsync(CancellationToken ct) =>
        await db.StorageLocationKinds.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code).Select(x => new CatalogItem(x.Id, x.Code)).ToArrayAsync(ct);
    public async Task<IReadOnlyCollection<CatalogItem>> GetStorageLocationTypesAsync(CancellationToken ct) =>
        await db.StorageLocationTypes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code).Select(x => new CatalogItem(x.Id, x.Code)).ToArrayAsync(ct);

    public async Task<MasterDataResult<StorageLocationDetails>> CreateStorageLocationAsync(SaveStorageLocationCommand command, CancellationToken ct)
    {
        var validation = await ValidateStorage(command, null, ct); if (validation is not null) return validation;
        var entity = NewStorage(command); db.StorageLocations.Add(entity);
        try { await db.SaveChangesAsync(ct); return MasterDataResult<StorageLocationDetails>.Success(StorageDetails(entity)); }
        catch (DbUpdateException) { return Conflict<StorageLocationDetails>("A storage location with this code already exists."); }
    }

    public async Task<MasterDataResult<StorageLocationDetails>> UpdateStorageLocationAsync(Guid id, SaveStorageLocationCommand command, CancellationToken ct)
    {
        var entity = await db.StorageLocations.Include(x => x.TypeAssignments).SingleOrDefaultAsync(x => x.Id == id, ct); if (entity is null) return NotFound<StorageLocationDetails>();
        var validation = await ValidateStorage(command, id, ct); if (validation is not null) return validation;
        if (!SetVersion(entity, command.RowVersion)) return Validation<StorageLocationDetails>("Row version is required.");
        entity.Code = NormalizeCode(command.Code); entity.Name = command.Name.Trim(); entity.Description = Clean(command.Description);
        entity.DepartmentId = command.DepartmentId; entity.KindId = command.KindId; entity.ParentStorageLocationId = command.ParentStorageLocationId;
        var desiredTypes = command.TypeIds.Distinct().ToHashSet();
        foreach (var assignment in entity.TypeAssignments.Where(x => !desiredTypes.Contains(x.StorageLocationTypeId)).ToArray())
            db.Remove(assignment);
        var existingTypes = entity.TypeAssignments.Select(x => x.StorageLocationTypeId).ToHashSet();
        foreach (var typeId in desiredTypes.Where(x => !existingTypes.Contains(x)))
            entity.TypeAssignments.Add(new() { StorageLocationTypeId = typeId });
        try { await db.SaveChangesAsync(ct); return MasterDataResult<StorageLocationDetails>.Success(StorageDetails(entity)); }
        catch (DbUpdateConcurrencyException) { return Conflict<StorageLocationDetails>("The storage location was changed by another request."); }
        catch (DbUpdateException) { return Conflict<StorageLocationDetails>("A storage location with this code already exists."); }
    }

    public async Task<MasterDataResult<bool>> SetStorageLocationActiveAsync(Guid id, bool active, CancellationToken ct)
    {
        var entity = await db.StorageLocations.SingleOrDefaultAsync(x => x.Id == id, ct); if (entity is null) return NotFound<bool>();
        if (!active && await db.StorageLocations.AnyAsync(x => x.ParentStorageLocationId == id && x.IsActive, ct))
            return MasterDataResult<bool>.Failure(MasterDataError.Dependency, "Deactivate child storage locations first.");
        entity.IsActive = active; await db.SaveChangesAsync(ct); return MasterDataResult<bool>.Success(true);
    }

    private async Task<MasterDataResult<DepartmentDetails>?> ValidateDepartment(SaveDepartmentCommand c, Guid? id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(c.Code) || string.IsNullOrWhiteSpace(c.Name)) return Validation<DepartmentDetails>("Code and name are required.");
        if (c.Code.Trim().Length != 4) return Validation<DepartmentDetails>("Department code must contain exactly 4 characters.");
        if (id is not null && c.ParentDepartmentId == id) return Validation<DepartmentDetails>("A department cannot be its own parent.");
        if (c.ParentDepartmentId is not null && !await db.Departments.AnyAsync(x => x.Id == c.ParentDepartmentId && x.IsActive, ct)) return Validation<DepartmentDetails>("The parent department must be active.");
        var parent = c.ParentDepartmentId; while (parent is not null) { if (parent == id) return Validation<DepartmentDetails>("The department hierarchy cannot contain a cycle."); parent = await db.Departments.Where(x => x.Id == parent).Select(x => x.ParentDepartmentId).SingleOrDefaultAsync(ct); }
        return null;
    }

    private async Task<MasterDataResult<StorageLocationDetails>?> ValidateStorage(SaveStorageLocationCommand c, Guid? id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(c.Code) || string.IsNullOrWhiteSpace(c.Name)) return Validation<StorageLocationDetails>("Code and name are required.");
        if (c.Code.Trim().Length != 4) return Validation<StorageLocationDetails>("Storage location code must contain exactly 4 characters.");
        if (c.TypeIds.Count == 0) return Validation<StorageLocationDetails>("At least one storage location type is required.");
        if (!await db.Departments.AnyAsync(x => x.Id == c.DepartmentId && x.IsActive, ct)) return Validation<StorageLocationDetails>("The department must be active.");
        if (!await db.StorageLocationKinds.AnyAsync(x => x.Id == c.KindId && x.IsActive, ct)) return Validation<StorageLocationDetails>("The storage location kind is invalid.");
        if (await db.StorageLocationTypes.CountAsync(x => c.TypeIds.Distinct().Contains(x.Id) && x.IsActive, ct) != c.TypeIds.Distinct().Count()) return Validation<StorageLocationDetails>("One or more storage location types are invalid.");
        if (id is not null && c.ParentStorageLocationId == id) return Validation<StorageLocationDetails>("A storage location cannot be its own parent.");
        if (c.ParentStorageLocationId is not null && !await db.StorageLocations.AnyAsync(x => x.Id == c.ParentStorageLocationId && x.IsActive, ct)) return Validation<StorageLocationDetails>("The parent storage location must be active.");
        var parent = c.ParentStorageLocationId; while (parent is not null) { if (parent == id) return Validation<StorageLocationDetails>("The storage location hierarchy cannot contain a cycle."); parent = await db.StorageLocations.Where(x => x.Id == parent).Select(x => x.ParentStorageLocationId).SingleOrDefaultAsync(ct); }
        return null;
    }

    private StorageLocation NewStorage(SaveStorageLocationCommand c)
    {
        var entity = new StorageLocation { Code = NormalizeCode(c.Code), Name = c.Name.Trim(), Description = Clean(c.Description), DepartmentId = c.DepartmentId, KindId = c.KindId, ParentStorageLocationId = c.ParentStorageLocationId };
        foreach (var typeId in c.TypeIds.Distinct()) entity.TypeAssignments.Add(new() { StorageLocationTypeId = typeId }); return entity;
    }
    private bool SetVersion(LeanProd.Domain.Common.AuditableEntity entity, string? version) { try { if (string.IsNullOrWhiteSpace(version)) return false; db.Entry(entity).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(version); return true; } catch (FormatException) { return false; } }
    private static DepartmentDetails DepartmentDetails(Department x) => new(x.Id, x.Code, x.Name, x.Description, x.ParentDepartmentId, x.IsActive, Convert.ToBase64String(x.RowVersion));
    private static StorageLocationDetails StorageDetails(StorageLocation x) => new(x.Id, x.Code, x.Name, x.Description, x.DepartmentId, x.KindId, x.ParentStorageLocationId, x.TypeAssignments.Select(a => a.StorageLocationTypeId).ToArray(), x.IsActive, Convert.ToBase64String(x.RowVersion));
    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static MasterDataResult<T> NotFound<T>() => MasterDataResult<T>.Failure(MasterDataError.NotFound, "Record was not found.");
    private static MasterDataResult<T> Validation<T>(string message) => MasterDataResult<T>.Failure(MasterDataError.Validation, message);
    private static MasterDataResult<T> Conflict<T>(string message) => MasterDataResult<T>.Failure(MasterDataError.Conflict, message);
}
