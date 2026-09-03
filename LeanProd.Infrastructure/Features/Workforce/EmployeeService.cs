using LeanProd.Application.Features.MasterData;
using LeanProd.Application.Features.Workforce;
using LeanProd.Domain.Workforce;
using LeanProd.Domain.Workforce.Policies;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.Workforce;

public sealed class EmployeeService(LeanProdDbContext db, TimeProvider timeProvider) : IEmployeeService
{
    public async Task<MasterDataPage<EmployeeSummary>> GetEmployeesAsync(
        WorkforceQuery query, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var source = db.Employees.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.PersonnelNumber.Contains(search) || x.LastName.Contains(search) ||
                x.FirstName.Contains(search) || (x.MiddleName != null && x.MiddleName.Contains(search)));
        }
        if (query.IsActive is not null) source = source.Where(x => x.IsActive == query.IsActive);
        if (query.DepartmentId is not null) source = source.Where(x => x.DepartmentId == query.DepartmentId);
        var total = await source.CountAsync(ct);
        var items = await source.OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new EmployeeSummary(x.Id, x.PersonnelNumber,
                x.LastName + " " + x.FirstName + (x.MiddleName == null ? "" : " " + x.MiddleName),
                x.Position, x.DepartmentId, x.Department == null ? null : x.Department.Name,
                x.BrigadeMemberships.Count(m => m.StartedAtUtc <= now &&
                    (m.EndedAtUtc == null || m.EndedAtUtc > now)), x.IsActive))
            .ToArrayAsync(ct);
        return new(items, query.Page, query.PageSize, total);
    }

    public async Task<EmployeeDetails?> GetEmployeeAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.Employees.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : Details(entity);
    }

    public async Task<IReadOnlyCollection<EmployeeOption>> GetEmployeeOptionsAsync(CancellationToken ct) =>
        await db.Employees.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Select(x => new EmployeeOption(x.Id, x.PersonnelNumber,
                x.LastName + " " + x.FirstName + (x.MiddleName == null ? "" : " " + x.MiddleName)))
            .ToArrayAsync(ct);

    public async Task<IReadOnlyCollection<BrigadeMembershipDetails>> GetBrigadeHistoryAsync(
        Guid id, CancellationToken ct)
    {
        var memberships = await db.BrigadeMemberships.AsNoTracking()
            .Include(x => x.Brigade).Include(x => x.Employee).Where(x => x.EmployeeId == id)
            .OrderByDescending(x => x.StartedAtUtc).ToArrayAsync(ct);
        return memberships.Select(MembershipDetails).ToArray();
    }

    public async Task<WorkforceResult<EmployeeDetails>> CreateEmployeeAsync(
        SaveEmployeeCommand command, CancellationToken ct)
    {
        var validation = await Validate(command, null, ct);
        if (validation is not null) return Validation<EmployeeDetails>(validation);
        var entity = new Employee();
        Apply(entity, command);
        db.Employees.Add(entity);
        try
        {
            await db.SaveChangesAsync(ct);
            return WorkforceResult<EmployeeDetails>.Success(Details(entity));
        }
        catch (DbUpdateException)
        {
            return Conflict<EmployeeDetails>("An employee with this personnel number already exists.");
        }
    }

    public async Task<WorkforceResult<EmployeeDetails>> UpdateEmployeeAsync(
        Guid id, SaveEmployeeCommand command, CancellationToken ct)
    {
        var entity = await db.Employees.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound<EmployeeDetails>();
        var validation = await Validate(command, id, ct);
        if (validation is not null) return Validation<EmployeeDetails>(validation);
        if (!SetVersion(entity, command.RowVersion)) return Validation<EmployeeDetails>("Row version is required.");
        Apply(entity, command);
        try
        {
            await db.SaveChangesAsync(ct);
            return WorkforceResult<EmployeeDetails>.Success(Details(entity));
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict<EmployeeDetails>("The employee was changed by another request.");
        }
        catch (DbUpdateException)
        {
            return Conflict<EmployeeDetails>("An employee with this personnel number already exists.");
        }
    }

    public async Task<WorkforceResult<bool>> SetEmployeeActiveAsync(Guid id, bool active, CancellationToken ct)
    {
        var entity = await db.Employees.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound<bool>();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (!active && await db.BrigadeMemberships.AnyAsync(x => x.EmployeeId == id &&
                x.StartedAtUtc <= now && (x.EndedAtUtc == null || x.EndedAtUtc > now), ct))
            return Dependency<bool>("Close active brigade memberships before deactivating the employee.");
        entity.IsActive = active;
        await db.SaveChangesAsync(ct);
        return WorkforceResult<bool>.Success(true);
    }

    private async Task<string?> Validate(SaveEmployeeCommand command, Guid? id, CancellationToken ct)
    {
        var error = WorkforcePolicy.ValidateEmployee(
            command.PersonnelNumber, command.LastName, command.FirstName);
        if (error is not null) return error;
        if (Clean(command.MiddleName)?.Length > 100 || Clean(command.Position)?.Length > 200)
            return "Middle name or position is too long.";
        if (command.DepartmentId is not null && !await db.Departments.AnyAsync(
                x => x.Id == command.DepartmentId && x.IsActive, ct))
            return "The department must be active.";
        return await db.Employees.AnyAsync(x => x.Id != id &&
                x.PersonnelNumber == NormalizeCode(command.PersonnelNumber), ct)
            ? "An employee with this personnel number already exists."
            : null;
    }

    private static void Apply(Employee entity, SaveEmployeeCommand command)
    {
        entity.PersonnelNumber = NormalizeCode(command.PersonnelNumber);
        entity.LastName = command.LastName.Trim();
        entity.FirstName = command.FirstName.Trim();
        entity.MiddleName = Clean(command.MiddleName);
        entity.Position = Clean(command.Position);
        entity.DepartmentId = command.DepartmentId;
    }

    private bool SetVersion(Employee entity, string? version)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(version)) return false;
            db.Entry(entity).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(version);
            return true;
        }
        catch (FormatException) { return false; }
    }

    private static EmployeeDetails Details(Employee x) => new(x.Id, x.PersonnelNumber, x.LastName,
        x.FirstName, x.MiddleName, x.Position, x.DepartmentId, x.IsActive,
        Convert.ToBase64String(x.RowVersion));
    private static BrigadeMembershipDetails MembershipDetails(BrigadeMembership x) => new(x.Id,
        x.BrigadeId, x.Brigade.Code, x.Brigade.Name, x.EmployeeId, x.Employee.PersonnelNumber,
        x.Employee.LastName + " " + x.Employee.FirstName, x.StartedAtUtc, x.EndedAtUtc,
        x.LaborParticipationCoefficient, Convert.ToBase64String(x.RowVersion));
    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static WorkforceResult<T> NotFound<T>() =>
        WorkforceResult<T>.Failure(WorkforceError.NotFound, "Record was not found.");
    private static WorkforceResult<T> Validation<T>(string message) =>
        WorkforceResult<T>.Failure(WorkforceError.Validation, message);
    private static WorkforceResult<T> Conflict<T>(string message) =>
        WorkforceResult<T>.Failure(WorkforceError.Conflict, message);
    private static WorkforceResult<T> Dependency<T>(string message) =>
        WorkforceResult<T>.Failure(WorkforceError.Dependency, message);
}
