using LeanProd.Application.Features.MasterData;
using LeanProd.Domain.MasterData;
using LeanProd.Domain.MasterData.Policies;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.MasterData;

public sealed class DepartmentService(LeanProdDbContext db) : IDepartmentService
{
    public async Task<MasterDataPage<DepartmentSummary>> GetDepartmentsAsync(
        MasterDataQuery query, CancellationToken ct)
    {
        var source = db.Departments.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
        }
        if (query.IsActive is not null) source = source.Where(x => x.IsActive == query.IsActive);
        var total = await source.CountAsync(ct);
        var items = await source.OrderBy(x => x.Code)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new DepartmentSummary(x.Id, x.Code, x.Name, x.ParentDepartmentId,
                x.ParentDepartment == null ? null : x.ParentDepartment.Name, x.IsActive))
            .ToArrayAsync(ct);
        return new(items, query.Page, query.PageSize, total);
    }

    public async Task<DepartmentDetails?> GetDepartmentAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.Departments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : Details(entity);
    }

    public async Task<IReadOnlyCollection<OptionItem>> GetDepartmentOptionsAsync(CancellationToken ct) =>
        await db.Departments.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new OptionItem(x.Id, x.Code, x.Name)).ToArrayAsync(ct);

    public async Task<MasterDataResult<DepartmentDetails>> CreateDepartmentAsync(
        SaveDepartmentCommand command, CancellationToken ct)
    {
        command = command with
        {
            Code = await CodeGenerator.EnsureCodeAsync(command.Code,
                db.Departments.AsNoTracking().Select(x => x.Code), 4, ct)
        };
        var validation = await Validate(command, null, ct);
        if (validation is not null) return Validation(validation);
        var entity = new Department
        {
            Code = NormalizeCode(command.Code),
            Name = command.Name.Trim(),
            Description = Clean(command.Description),
            ParentDepartmentId = command.ParentDepartmentId
        };
        db.Departments.Add(entity);
        try
        {
            await db.SaveChangesAsync(ct);
            return MasterDataResult<DepartmentDetails>.Success(Details(entity));
        }
        catch (DbUpdateException)
        {
            return Conflict("A department with this code already exists.");
        }
    }

    public async Task<MasterDataResult<DepartmentDetails>> UpdateDepartmentAsync(
        Guid id, SaveDepartmentCommand command, CancellationToken ct)
    {
        var entity = await db.Departments.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound<DepartmentDetails>();
        command = command with
        {
            Code = await CodeGenerator.EnsureCodeAsync(command.Code,
                db.Departments.AsNoTracking().Select(x => x.Code), 4, ct)
        };
        var validation = await Validate(command, id, ct);
        if (validation is not null) return Validation(validation);
        if (!SetVersion(entity, command.RowVersion)) return Validation("Row version is required.");
        entity.Code = NormalizeCode(command.Code);
        entity.Name = command.Name.Trim();
        entity.Description = Clean(command.Description);
        entity.ParentDepartmentId = command.ParentDepartmentId;
        try
        {
            await db.SaveChangesAsync(ct);
            return MasterDataResult<DepartmentDetails>.Success(Details(entity));
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("The department was changed by another request.");
        }
        catch (DbUpdateException)
        {
            return Conflict("A department with this code already exists.");
        }
    }

    public async Task<MasterDataResult<bool>> SetDepartmentActiveAsync(
        Guid id, bool active, CancellationToken ct)
    {
        var entity = await db.Departments.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound<bool>();
        var hasActiveDependants = !active &&
            (await db.Departments.AnyAsync(x => x.ParentDepartmentId == id && x.IsActive, ct) ||
             await db.StorageLocations.AnyAsync(x => x.DepartmentId == id && x.IsActive, ct));
        if (hasActiveDependants)
            return MasterDataResult<bool>.Failure(MasterDataError.Dependency,
                "Deactivate child departments and storage locations first.");
        entity.IsActive = active;
        await db.SaveChangesAsync(ct);
        return MasterDataResult<bool>.Success(true);
    }

    private async Task<string?> Validate(SaveDepartmentCommand command, Guid? id, CancellationToken ct)
    {
        var error = DepartmentPolicy.ValidateInput(command.Code, command.Name);
        if (error is not null) return error;
        if (command.ParentDepartmentId is not null &&
            !await db.Departments.AnyAsync(x => x.Id == command.ParentDepartmentId && x.IsActive, ct))
            return "The parent department must be active.";
        var ancestors = await LoadAncestorIds(command.ParentDepartmentId, ct);
        return HierarchyPolicy.Validate(id, command.ParentDepartmentId, ancestors, "A department");
    }

    private async Task<IReadOnlyCollection<Guid>> LoadAncestorIds(Guid? parentId, CancellationToken ct)
    {
        var ancestors = new List<Guid>();
        while (parentId is not null)
        {
            ancestors.Add(parentId.Value);
            parentId = await db.Departments.Where(x => x.Id == parentId)
                .Select(x => x.ParentDepartmentId).SingleOrDefaultAsync(ct);
        }
        return ancestors;
    }

    private bool SetVersion(Department entity, string? version)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(version)) return false;
            db.Entry(entity).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(version);
            return true;
        }
        catch (FormatException) { return false; }
    }

    private static DepartmentDetails Details(Department x) =>
        new(x.Id, x.Code, x.Name, x.Description, x.ParentDepartmentId, x.IsActive,
            Convert.ToBase64String(x.RowVersion));
    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static MasterDataResult<T> NotFound<T>() =>
        MasterDataResult<T>.Failure(MasterDataError.NotFound, "Record was not found.");
    private static MasterDataResult<DepartmentDetails> Validation(string message) =>
        MasterDataResult<DepartmentDetails>.Failure(MasterDataError.Validation, message);
    private static MasterDataResult<DepartmentDetails> Conflict(string message) =>
        MasterDataResult<DepartmentDetails>.Failure(MasterDataError.Conflict, message);
}
