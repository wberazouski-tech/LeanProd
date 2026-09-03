using LeanProd.Application.Features.MasterData;
using LeanProd.Application.Features.Workforce;
using LeanProd.Domain.Workforce;
using LeanProd.Domain.Workforce.Policies;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.Workforce;

public sealed class BrigadeService(LeanProdDbContext db, TimeProvider timeProvider) : IBrigadeService
{
    public async Task<MasterDataPage<BrigadeSummary>> GetBrigadesAsync(
        WorkforceQuery query, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var source = db.Brigades.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
        }
        if (query.IsActive is not null) source = source.Where(x => x.IsActive == query.IsActive);
        if (query.DepartmentId is not null) source = source.Where(x => x.DepartmentId == query.DepartmentId);
        var total = await source.CountAsync(ct);
        var items = await source.OrderBy(x => x.Code)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new BrigadeSummary(x.Id, x.Code, x.Name, x.DepartmentId,
                x.Department == null ? null : x.Department.Name,
                x.Memberships.Count(m => m.StartedAtUtc <= now &&
                    (m.EndedAtUtc == null || m.EndedAtUtc > now)), x.IsActive))
            .ToArrayAsync(ct);
        return new(items, query.Page, query.PageSize, total);
    }

    public async Task<BrigadeDetails?> GetBrigadeAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.Brigades.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : Details(entity);
    }

    public async Task<IReadOnlyCollection<BrigadeOption>> GetBrigadeOptionsAsync(CancellationToken ct) =>
        await db.Brigades.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code)
            .Select(x => new BrigadeOption(x.Id, x.Code, x.Name)).ToArrayAsync(ct);

    public async Task<IReadOnlyCollection<BrigadeMembershipDetails>> GetMembershipsAsync(
        Guid brigadeId, bool history, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var query = db.BrigadeMemberships.AsNoTracking().Where(x => x.BrigadeId == brigadeId);
        if (!history)
            query = query.Where(x => x.StartedAtUtc <= now && (x.EndedAtUtc == null || x.EndedAtUtc > now));
        var memberships = await query.Include(x => x.Brigade).Include(x => x.Employee)
            .OrderByDescending(x => x.StartedAtUtc).ToArrayAsync(ct);
        return memberships.Select(MembershipDetails).ToArray();
    }

    public async Task<WorkforceResult<BrigadeDetails>> CreateBrigadeAsync(
        SaveBrigadeCommand command, CancellationToken ct)
    {
        command = command with
        {
            Code = await CodeGenerator.EnsureCodeAsync(command.Code,
                db.Brigades.AsNoTracking().Select(x => x.Code), 4, ct)
        };
        var validation = await Validate(command, null, ct);
        if (validation is not null) return Validation<BrigadeDetails>(validation);
        var entity = new Brigade();
        Apply(entity, command);
        db.Brigades.Add(entity);
        try
        {
            await db.SaveChangesAsync(ct);
            return WorkforceResult<BrigadeDetails>.Success(Details(entity));
        }
        catch (DbUpdateException)
        {
            return Conflict<BrigadeDetails>("A brigade with this code already exists.");
        }
    }

    public async Task<WorkforceResult<BrigadeDetails>> UpdateBrigadeAsync(
        Guid id, SaveBrigadeCommand command, CancellationToken ct)
    {
        var entity = await db.Brigades.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound<BrigadeDetails>();
        command = command with
        {
            Code = await CodeGenerator.EnsureCodeAsync(command.Code,
                db.Brigades.AsNoTracking().Select(x => x.Code), 4, ct)
        };
        var validation = await Validate(command, id, ct);
        if (validation is not null) return Validation<BrigadeDetails>(validation);
        if (!SetVersion(entity, command.RowVersion)) return Validation<BrigadeDetails>("Row version is required.");
        Apply(entity, command);
        try
        {
            await db.SaveChangesAsync(ct);
            return WorkforceResult<BrigadeDetails>.Success(Details(entity));
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict<BrigadeDetails>("The brigade was changed by another request.");
        }
        catch (DbUpdateException)
        {
            return Conflict<BrigadeDetails>("A brigade with this code already exists.");
        }
    }

    public async Task<WorkforceResult<bool>> SetBrigadeActiveAsync(Guid id, bool active, CancellationToken ct)
    {
        var entity = await db.Brigades.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound<bool>();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (!active && await db.BrigadeMemberships.AnyAsync(x => x.BrigadeId == id &&
                x.StartedAtUtc <= now && (x.EndedAtUtc == null || x.EndedAtUtc > now), ct))
            return Dependency<bool>("Close active memberships before deactivating the brigade.");
        entity.IsActive = active;
        await db.SaveChangesAsync(ct);
        return WorkforceResult<bool>.Success(true);
    }

    public async Task<WorkforceResult<BrigadeMembershipDetails>> AddMembershipAsync(
        Guid brigadeId, AddBrigadeMembershipCommand command, CancellationToken ct)
    {
        var error = BrigadeMembershipPolicy.Validate(command.StartedAtUtc, command.EndedAtUtc,
            command.LaborParticipationCoefficient);
        if (error is not null) return Validation<BrigadeMembershipDetails>(error);
        if (!await db.Brigades.AnyAsync(x => x.Id == brigadeId && x.IsActive, ct))
            return Validation<BrigadeMembershipDetails>("An active brigade is required.");
        if (!await db.Employees.AnyAsync(x => x.Id == command.EmployeeId && x.IsActive, ct))
            return Validation<BrigadeMembershipDetails>("An active employee is required.");
        if (await HasOverlap(brigadeId, command.EmployeeId, command.StartedAtUtc, command.EndedAtUtc, null, ct))
            return Conflict<BrigadeMembershipDetails>("The employee already belongs to this brigade during the selected period.");
        var entity = new BrigadeMembership
        {
            BrigadeId = brigadeId,
            EmployeeId = command.EmployeeId,
            StartedAtUtc = command.StartedAtUtc,
            EndedAtUtc = command.EndedAtUtc,
            LaborParticipationCoefficient = command.LaborParticipationCoefficient
        };
        db.BrigadeMemberships.Add(entity);
        try
        {
            await db.SaveChangesAsync(ct);
            return WorkforceResult<BrigadeMembershipDetails>.Success((await LoadMembership(entity.Id, ct))!);
        }
        catch (DbUpdateException)
        {
            return Conflict<BrigadeMembershipDetails>("The membership conflicts with another change.");
        }
    }

    public async Task<WorkforceResult<BrigadeMembershipDetails>> CloseMembershipAsync(
        Guid brigadeId, Guid membershipId, CloseBrigadeMembershipCommand command, CancellationToken ct)
    {
        var entity = await db.BrigadeMemberships.SingleOrDefaultAsync(
            x => x.Id == membershipId && x.BrigadeId == brigadeId, ct);
        if (entity is null) return NotFound<BrigadeMembershipDetails>();
        var error = BrigadeMembershipPolicy.Validate(entity.StartedAtUtc, command.EndedAtUtc,
            entity.LaborParticipationCoefficient);
        if (error is not null) return Validation<BrigadeMembershipDetails>(error);
        if (!SetVersion(entity, command.RowVersion))
            return Validation<BrigadeMembershipDetails>("Row version is required.");
        entity.EndedAtUtc = command.EndedAtUtc;
        try
        {
            await db.SaveChangesAsync(ct);
            return WorkforceResult<BrigadeMembershipDetails>.Success((await LoadMembership(entity.Id, ct))!);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict<BrigadeMembershipDetails>("The membership was changed by another request.");
        }
    }

    public async Task<WorkforceResult<BrigadeMembershipDetails>> ChangeCoefficientAsync(
        Guid brigadeId, Guid membershipId, ChangeMembershipCoefficientCommand command, CancellationToken ct)
    {
        var entity = await db.BrigadeMemberships.SingleOrDefaultAsync(
            x => x.Id == membershipId && x.BrigadeId == brigadeId, ct);
        if (entity is null) return NotFound<BrigadeMembershipDetails>();
        if (command.EffectiveFromUtc <= entity.StartedAtUtc ||
            (entity.EndedAtUtc is not null && command.EffectiveFromUtc >= entity.EndedAtUtc))
            return Validation<BrigadeMembershipDetails>("The coefficient effective date must be inside the membership interval.");
        var error = BrigadeMembershipPolicy.Validate(command.EffectiveFromUtc, entity.EndedAtUtc,
            command.LaborParticipationCoefficient);
        if (error is not null) return Validation<BrigadeMembershipDetails>(error);
        if (!SetVersion(entity, command.RowVersion))
            return Validation<BrigadeMembershipDetails>("Row version is required.");

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var previousEnd = entity.EndedAtUtc;
            entity.EndedAtUtc = command.EffectiveFromUtc;
            var replacement = new BrigadeMembership
            {
                BrigadeId = entity.BrigadeId,
                EmployeeId = entity.EmployeeId,
                StartedAtUtc = command.EffectiveFromUtc,
                EndedAtUtc = previousEnd,
                LaborParticipationCoefficient = command.LaborParticipationCoefficient
            };
            db.BrigadeMemberships.Add(replacement);
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return WorkforceResult<BrigadeMembershipDetails>.Success((await LoadMembership(replacement.Id, ct))!);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(ct);
            return Conflict<BrigadeMembershipDetails>("The membership was changed by another request.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(ct);
            return Conflict<BrigadeMembershipDetails>("The coefficient change conflicts with another interval.");
        }
    }

    private async Task<bool> HasOverlap(Guid brigadeId, Guid employeeId, DateTime start,
        DateTime? end, Guid? excludedId, CancellationToken ct)
    {
        var upperBound = end ?? DateTime.MaxValue;
        return await db.BrigadeMemberships.AnyAsync(x => x.BrigadeId == brigadeId &&
            x.EmployeeId == employeeId && x.Id != excludedId && x.StartedAtUtc < upperBound &&
            (x.EndedAtUtc == null || x.EndedAtUtc > start), ct);
    }

    private async Task<string?> Validate(SaveBrigadeCommand command, Guid? id, CancellationToken ct)
    {
        var error = WorkforcePolicy.ValidateBrigade(command.Code, command.Name);
        if (error is not null) return error;
        if (Clean(command.Description)?.Length > 1000) return "Description cannot exceed 1000 characters.";
        if (command.DepartmentId is not null && !await db.Departments.AnyAsync(
                x => x.Id == command.DepartmentId && x.IsActive, ct))
            return "The department must be active.";
        return await db.Brigades.AnyAsync(x => x.Id != id && x.Code == NormalizeCode(command.Code), ct)
            ? "A brigade with this code already exists."
            : null;
    }

    private async Task<BrigadeMembershipDetails?> LoadMembership(Guid id, CancellationToken ct)
    {
        var entity = await db.BrigadeMemberships.AsNoTracking()
            .Include(x => x.Brigade).Include(x => x.Employee).SingleOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : MembershipDetails(entity);
    }

    private static void Apply(Brigade entity, SaveBrigadeCommand command)
    {
        entity.Code = NormalizeCode(command.Code);
        entity.Name = command.Name.Trim();
        entity.Description = Clean(command.Description);
        entity.DepartmentId = command.DepartmentId;
    }

    private bool SetVersion(Brigade entity, string? version) => SetVersion((object)entity, version);
    private bool SetVersion(BrigadeMembership entity, string? version) => SetVersion((object)entity, version);
    private bool SetVersion(object entity, string? version)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(version)) return false;
            db.Entry(entity).Property(nameof(LeanProd.Domain.Common.AuditableEntity.RowVersion)).OriginalValue =
                Convert.FromBase64String(version);
            return true;
        }
        catch (FormatException) { return false; }
    }

    private static BrigadeDetails Details(Brigade x) => new(x.Id, x.Code, x.Name, x.Description,
        x.DepartmentId, x.IsActive, Convert.ToBase64String(x.RowVersion));
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
