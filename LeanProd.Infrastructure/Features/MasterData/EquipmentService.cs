using LeanProd.Application.Features.MasterData;
using LeanProd.Domain.MasterData;
using LeanProd.Domain.MasterData.Policies;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.MasterData;

public sealed class EquipmentService(LeanProdDbContext db) : IEquipmentService
{
    public async Task<MasterDataPage<EquipmentSummary>> GetEquipmentAsync(EquipmentQuery query, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var source = db.Equipment.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var value = query.Search.Trim();
            source = source.Where(x => x.Name.Contains(value) || (x.InventoryNumber != null && x.InventoryNumber.Contains(value)) ||
                (x.SerialNumber != null && x.SerialNumber.Contains(value)) || (x.Manufacturer != null && x.Manufacturer.Contains(value)) ||
                (x.Model != null && x.Model.Contains(value)));
        }
        if (query.IsActive is not null) source = source.Where(x => x.IsActive == query.IsActive);
        if (query.DepartmentId is not null) source = source.Where(x => x.DepartmentId == query.DepartmentId);
        if (query.EquipmentTypeId is not null) source = source.Where(x => x.EquipmentTypeId == query.EquipmentTypeId);
        if (!string.IsNullOrWhiteSpace(query.State)) source = source.Where(x => x.StateEvents.Any(e =>
            e.StartedAtUtc <= now && (e.EndedAtUtc == null || e.EndedAtUtc > now) && e.State == query.State));
        var total = await source.CountAsync(ct);
        var items = await source.OrderBy(x => x.Name).ThenBy(x => x.InventoryNumber)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new EquipmentSummary(x.Id, x.Name, x.InventoryNumber,
                x.EquipmentType == null ? null : x.EquipmentType.Name, x.Department.Name,
                x.ParentEquipment == null ? null : x.ParentEquipment.Name,
                x.ParentEquipment == null ? null : x.ParentEquipment.InventoryNumber,
                x.StateEvents.Where(e => e.StartedAtUtc <= now && (e.EndedAtUtc == null || e.EndedAtUtc > now))
                    .Select(e => e.State).SingleOrDefault(), x.IsActive))
            .ToArrayAsync(ct);
        return new(items, query.Page, query.PageSize, total);
    }

    public async Task<EquipmentDetails?> GetEquipmentAsync(Guid id, CancellationToken ct)
    {
        var item = await db.Equipment.AsNoTracking().Include(x => x.StateEvents).SingleOrDefaultAsync(x => x.Id == id, ct);
        return item is null ? null : Details(item);
    }

    public async Task<IReadOnlyCollection<EquipmentOption>> GetEquipmentOptionsAsync(CancellationToken ct) =>
        await db.Equipment.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ThenBy(x => x.InventoryNumber)
            .Select(x => new EquipmentOption(x.Id, x.Name, x.InventoryNumber, x.DepartmentId)).ToArrayAsync(ct);

    public async Task<MasterDataResult<EquipmentDetails>> CreateEquipmentAsync(SaveEquipmentCommand command, CancellationToken ct)
    {
        var validation = await ValidateEquipment(command, null, ct); if (validation is not null) return validation;
        var item = NewEquipment(command); db.Equipment.Add(item);
        try { await db.SaveChangesAsync(ct); return MasterDataResult<EquipmentDetails>.Success(Details(item)); }
        catch (DbUpdateException) { return Conflict<EquipmentDetails>("An equipment item with this inventory number already exists."); }
    }

    public async Task<MasterDataResult<EquipmentDetails>> UpdateEquipmentAsync(Guid id, SaveEquipmentCommand command, CancellationToken ct)
    {
        var item = await db.Equipment.Include(x => x.StateEvents).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return NotFound<EquipmentDetails>();
        var validation = await ValidateEquipment(command, id, ct); if (validation is not null) return validation;
        if (!SetVersion(item, command.RowVersion)) return Validation<EquipmentDetails>("Row version is required.");
        item.Name = command.Name.Trim(); item.InventoryNumber = Clean(command.InventoryNumber);
        item.EquipmentTypeId = command.EquipmentTypeId; item.DepartmentId = command.DepartmentId;
        item.ParentEquipmentId = command.ParentEquipmentId; item.SerialNumber = Clean(command.SerialNumber);
        item.Manufacturer = Clean(command.Manufacturer); item.Model = Clean(command.Model);
        item.CommissionedOn = command.CommissionedOn; item.Description = Clean(command.Description);
        try { await db.SaveChangesAsync(ct); return MasterDataResult<EquipmentDetails>.Success(Details(item)); }
        catch (DbUpdateConcurrencyException) { return Conflict<EquipmentDetails>("The equipment item was changed by another request."); }
        catch (DbUpdateException) { return Conflict<EquipmentDetails>("An equipment item with this inventory number already exists."); }
    }

    public async Task<MasterDataResult<bool>> SetEquipmentActiveAsync(Guid id, bool active, CancellationToken ct)
    {
        var item = await db.Equipment.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound<bool>();
        if (!active && await db.Equipment.AnyAsync(x => x.ParentEquipmentId == id && x.IsActive, ct))
            return MasterDataResult<bool>.Failure(MasterDataError.Dependency, "Deactivate child equipment first.");
        item.IsActive = active; await db.SaveChangesAsync(ct); return MasterDataResult<bool>.Success(true);
    }

    public async Task<IReadOnlyCollection<EquipmentTypeDetails>> GetEquipmentTypesAsync(bool activeOnly, CancellationToken ct)
    {
        var source = db.EquipmentTypes.AsNoTracking(); if (activeOnly) source = source.Where(x => x.IsActive);
        return await source.OrderBy(x => x.Name).Select(x => new EquipmentTypeDetails(x.Id, x.Name, x.Description,
            x.IsActive, Convert.ToBase64String(x.RowVersion))).ToArrayAsync(ct);
    }

    public async Task<MasterDataResult<EquipmentTypeDetails>> CreateEquipmentTypeAsync(SaveEquipmentTypeCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Name)) return Validation<EquipmentTypeDetails>("Name is required.");
        var item = new EquipmentType { Name = command.Name.Trim(), Description = Clean(command.Description), IsActive = command.IsActive }; db.EquipmentTypes.Add(item);
        try { await db.SaveChangesAsync(ct); return MasterDataResult<EquipmentTypeDetails>.Success(TypeDetails(item)); }
        catch (DbUpdateException) { return Conflict<EquipmentTypeDetails>("An equipment type with this name already exists."); }
    }

    public async Task<MasterDataResult<EquipmentTypeDetails>> UpdateEquipmentTypeAsync(Guid id, SaveEquipmentTypeCommand command, CancellationToken ct)
    {
        var item = await db.EquipmentTypes.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound<EquipmentTypeDetails>();
        if (string.IsNullOrWhiteSpace(command.Name)) return Validation<EquipmentTypeDetails>("Name is required.");
        if (!SetVersion(item, command.RowVersion)) return Validation<EquipmentTypeDetails>("Row version is required.");
        if (!command.IsActive && item.IsActive && await db.Equipment.AnyAsync(x => x.EquipmentTypeId == id && x.IsActive, ct))
            return MasterDataResult<EquipmentTypeDetails>.Failure(MasterDataError.Dependency, "Remove this type from active equipment first.");
        item.Name = command.Name.Trim(); item.Description = Clean(command.Description); item.IsActive = command.IsActive;
        try { await db.SaveChangesAsync(ct); return MasterDataResult<EquipmentTypeDetails>.Success(TypeDetails(item)); }
        catch (DbUpdateConcurrencyException) { return Conflict<EquipmentTypeDetails>("The equipment type was changed by another request."); }
        catch (DbUpdateException) { return Conflict<EquipmentTypeDetails>("An equipment type with this name already exists."); }
    }

    public async Task<MasterDataResult<bool>> SetEquipmentTypeActiveAsync(Guid id, bool active, CancellationToken ct)
    {
        var item = await db.EquipmentTypes.SingleOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound<bool>();
        if (!active && await db.Equipment.AnyAsync(x => x.EquipmentTypeId == id && x.IsActive, ct))
            return MasterDataResult<bool>.Failure(MasterDataError.Dependency, "Remove this type from active equipment first.");
        item.IsActive = active; await db.SaveChangesAsync(ct); return MasterDataResult<bool>.Success(true);
    }

    public async Task<IReadOnlyCollection<EquipmentStateEventDetails>> GetStateHistoryAsync(Guid equipmentId, CancellationToken ct) =>
        await db.EquipmentStateEvents.AsNoTracking().Where(x => x.EquipmentId == equipmentId)
            .OrderBy(x => x.StartedAtUtc).Select(x => new EquipmentStateEventDetails(x.Id, x.State,
                x.StartedAtUtc, x.EndedAtUtc, x.Comment, Convert.ToBase64String(x.RowVersion))).ToArrayAsync(ct);

    public async Task<MasterDataResult<EquipmentStateEventDetails>> ChangeStateAsync(Guid equipmentId, ChangeEquipmentStateCommand command, CancellationToken ct)
    {
        var inputValidation = EquipmentPolicy.ValidateState(command.State, command.StartedAtUtc, command.EndedAtUtc);
        if (inputValidation is not null) return Validation<EquipmentStateEventDetails>(inputValidation);
        if (!await db.Equipment.AnyAsync(x => x.Id == equipmentId, ct)) return NotFound<EquipmentStateEventDetails>();
        var state = NormalizeState(command.State);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            if (await db.EquipmentStateEvents.AnyAsync(x => x.EquipmentId == equipmentId && x.StartedAtUtc == command.StartedAtUtc, ct))
                return Validation<EquipmentStateEventDetails>("A state event already starts at this time.");
            var previous = await db.EquipmentStateEvents.Where(x => x.EquipmentId == equipmentId && x.StartedAtUtc < command.StartedAtUtc)
                .OrderByDescending(x => x.StartedAtUtc).FirstOrDefaultAsync(ct);
            var nextEvent = await db.EquipmentStateEvents.Where(x => x.EquipmentId == equipmentId && x.StartedAtUtc > command.StartedAtUtc)
                .OrderBy(x => x.StartedAtUtc).FirstOrDefaultAsync(ct);
            if (previous is not null && previous.State == state && (previous.EndedAtUtc is null || previous.EndedAtUtc > command.StartedAtUtc))
                return Validation<EquipmentStateEventDetails>("The equipment already has this state at the selected time.");
            var endValidation = EquipmentPolicy.ValidateStateEnd(
                command.StartedAtUtc, command.EndedAtUtc, nextEvent?.StartedAtUtc);
            if (endValidation is not null) return Validation<EquipmentStateEventDetails>(endValidation);
            if (previous is not null && (previous.EndedAtUtc is null || previous.EndedAtUtc > command.StartedAtUtc))
                previous.EndedAtUtc = command.StartedAtUtc;
            var next = new EquipmentStateEvent
            {
                EquipmentId = equipmentId,
                State = state,
                StartedAtUtc = command.StartedAtUtc,
                EndedAtUtc = command.EndedAtUtc ?? nextEvent?.StartedAtUtc,
                Comment = Clean(command.Comment)
            };
            db.EquipmentStateEvents.Add(next); await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
            return MasterDataResult<EquipmentStateEventDetails>.Success(EventDetails(next));
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(ct);
            return Conflict<EquipmentStateEventDetails>("The equipment state was changed by another request.");
        }
    }

    public async Task<MasterDataResult<EquipmentStateEventDetails>> UpdateStateAsync(Guid equipmentId, Guid eventId, ChangeEquipmentStateCommand command, CancellationToken ct)
    {
        var inputValidation = EquipmentPolicy.ValidateState(command.State, command.StartedAtUtc, command.EndedAtUtc);
        if (inputValidation is not null) return Validation<EquipmentStateEventDetails>(inputValidation);
        var item = await db.EquipmentStateEvents.SingleOrDefaultAsync(x => x.Id == eventId && x.EquipmentId == equipmentId, ct);
        if (item is null) return NotFound<EquipmentStateEventDetails>();
        if (!SetVersion(item, command.RowVersion)) return Validation<EquipmentStateEventDetails>("Row version is required.");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            if (await db.EquipmentStateEvents.AnyAsync(x => x.EquipmentId == equipmentId && x.Id != eventId && x.StartedAtUtc == command.StartedAtUtc, ct))
                return Validation<EquipmentStateEventDetails>("A state event already starts at this time.");
            var previous = await db.EquipmentStateEvents.Where(x => x.EquipmentId == equipmentId && x.Id != eventId && x.StartedAtUtc < command.StartedAtUtc)
                .OrderByDescending(x => x.StartedAtUtc).FirstOrDefaultAsync(ct);
            var nextEvent = await db.EquipmentStateEvents.Where(x => x.EquipmentId == equipmentId && x.Id != eventId && x.StartedAtUtc > command.StartedAtUtc)
                .OrderBy(x => x.StartedAtUtc).FirstOrDefaultAsync(ct);
            var endValidation = EquipmentPolicy.ValidateStateEnd(
                command.StartedAtUtc, command.EndedAtUtc, nextEvent?.StartedAtUtc);
            if (endValidation is not null) return Validation<EquipmentStateEventDetails>(endValidation);
            if (previous is not null && (previous.EndedAtUtc is null || previous.EndedAtUtc > command.StartedAtUtc))
                previous.EndedAtUtc = command.StartedAtUtc;
            item.State = NormalizeState(command.State); item.StartedAtUtc = command.StartedAtUtc;
            item.EndedAtUtc = command.EndedAtUtc ?? nextEvent?.StartedAtUtc; item.Comment = Clean(command.Comment);
            await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
            return MasterDataResult<EquipmentStateEventDetails>.Success(EventDetails(item));
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(ct);
            return Conflict<EquipmentStateEventDetails>("The state interval was changed by another request.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(ct);
            return Conflict<EquipmentStateEventDetails>("The state interval conflicts with another interval.");
        }
    }

    private static string NormalizeState(string state) => EquipmentStateCodes.All.Single(x => x.Equals(state, StringComparison.OrdinalIgnoreCase));
    private static EquipmentStateEventDetails EventDetails(EquipmentStateEvent x) => new(x.Id, x.State,
        x.StartedAtUtc, x.EndedAtUtc, x.Comment, Convert.ToBase64String(x.RowVersion));

    private async Task<MasterDataResult<EquipmentDetails>?> ValidateEquipment(SaveEquipmentCommand c, Guid? id, CancellationToken ct)
    {
        var error = EquipmentPolicy.ValidateInput(c.Name, c.InventoryNumber);
        if (error is not null) return Validation<EquipmentDetails>(error);
        var departmentIsActive = await db.Departments.AnyAsync(
            x => x.Id == c.DepartmentId && x.IsActive, ct);
        var equipmentTypeIsActive = c.EquipmentTypeId is null || await db.EquipmentTypes.AnyAsync(
            x => x.Id == c.EquipmentTypeId && x.IsActive, ct);
        var parentIsActive = c.ParentEquipmentId is null || await db.Equipment.AnyAsync(
            x => x.Id == c.ParentEquipmentId && x.IsActive, ct);
        error = EquipmentPolicy.ValidateReferences(
            departmentIsActive, equipmentTypeIsActive, parentIsActive);
        if (error is not null) return Validation<EquipmentDetails>(error);
        var ancestors = new List<Guid>();
        var parent = c.ParentEquipmentId;
        while (parent is not null)
        {
            ancestors.Add(parent.Value);
            parent = await db.Equipment.Where(x => x.Id == parent)
                .Select(x => x.ParentEquipmentId).SingleOrDefaultAsync(ct);
        }
        error = HierarchyPolicy.Validate(id, c.ParentEquipmentId, ancestors, "Equipment");
        return error is null ? null : Validation<EquipmentDetails>(error);
    }
    private static Equipment NewEquipment(SaveEquipmentCommand c) => new() { Name = c.Name.Trim(), InventoryNumber = Clean(c.InventoryNumber), EquipmentTypeId = c.EquipmentTypeId, DepartmentId = c.DepartmentId, ParentEquipmentId = c.ParentEquipmentId, SerialNumber = Clean(c.SerialNumber), Manufacturer = Clean(c.Manufacturer), Model = Clean(c.Model), CommissionedOn = c.CommissionedOn, Description = Clean(c.Description) };
    private static EquipmentDetails Details(Equipment x)
    {
        var now = DateTime.UtcNow;
        return new(x.Id, x.Name, x.InventoryNumber, x.EquipmentTypeId, x.DepartmentId, x.ParentEquipmentId,
            x.SerialNumber, x.Manufacturer, x.Model, x.CommissionedOn, x.Description,
            x.StateEvents.SingleOrDefault(e => e.StartedAtUtc <= now && (e.EndedAtUtc == null || e.EndedAtUtc > now))?.State,
            x.IsActive, Convert.ToBase64String(x.RowVersion));
    }
    private static EquipmentTypeDetails TypeDetails(EquipmentType x) => new(x.Id, x.Name, x.Description, x.IsActive, Convert.ToBase64String(x.RowVersion));
    private bool SetVersion(LeanProd.Domain.Common.AuditableEntity item, string? version) { try { if (string.IsNullOrWhiteSpace(version)) return false; db.Entry(item).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(version); return true; } catch (FormatException) { return false; } }
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static MasterDataResult<T> NotFound<T>() => MasterDataResult<T>.Failure(MasterDataError.NotFound, "Record was not found.");
    private static MasterDataResult<T> Validation<T>(string message) => MasterDataResult<T>.Failure(MasterDataError.Validation, message);
    private static MasterDataResult<T> Conflict<T>(string message) => MasterDataResult<T>.Failure(MasterDataError.Conflict, message);
}
