using LeanProd.Application.Features.Scheduling;
using LeanProd.Domain.Common;
using LeanProd.Domain.Scheduling;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.Scheduling;

public sealed class SchedulingService(LeanProdDbContext db) : ISchedulingService
{
    public async Task<IReadOnlyCollection<WorkScheduleSummary>> GetSchedulesAsync(CancellationToken ct) =>
        await db.WorkSchedules.AsNoTracking().OrderBy(x => x.Kind).ThenBy(x => x.Code)
            .Select(x => new WorkScheduleSummary(x.Id, x.Code, x.Name, x.Kind, x.Status,
                x.CycleType, x.CycleLengthDays, x.Departments.Count)).ToArrayAsync(ct);

    public async Task<WorkScheduleDetails?> GetScheduleAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.WorkSchedules.AsNoTracking().AsSplitQuery()
            .Include(x => x.Departments).Include(x => x.Days).ThenInclude(x => x.Intervals)
            .ThenInclude(x => x.WorkShift).SingleOrDefaultAsync(x => x.Id == id, ct);
        return entity is null ? null : Details(entity);
    }

    public async Task<SchedulingResult<WorkScheduleDetails>> SaveScheduleAsync(
        Guid? id, SaveWorkScheduleCommand command, CancellationToken ct)
    {
        var validation = Validate(command);
        if (validation is not null) return Validation<WorkScheduleDetails>(validation);
        if (command.Kind == WorkScheduleKind.Main && command.Status == WorkScheduleStatus.Active &&
            await db.WorkSchedules.AnyAsync(x => x.Id != id && x.Kind == WorkScheduleKind.Main &&
                x.Status == WorkScheduleStatus.Active, ct))
            return Conflict<WorkScheduleDetails>("Only one active main work schedule is allowed.");
        var departmentIds = command.DepartmentIds.Distinct().ToArray();
        if (command.Kind == WorkScheduleKind.Main && departmentIds.Length > 0)
            return Validation<WorkScheduleDetails>("The main schedule is inherited and cannot be assigned directly.");
        if (command.Status != WorkScheduleStatus.Active && departmentIds.Length > 0)
            return Validation<WorkScheduleDetails>("Only an active optional schedule can be assigned to departments.");
        if (departmentIds.Length != await db.Departments.CountAsync(x => departmentIds.Contains(x.Id) && x.IsActive, ct))
            return Validation<WorkScheduleDetails>("Every assigned department must be active.");
        var shiftIds = command.Days.SelectMany(x => x.Intervals).Select(x => x.WorkShiftId).Distinct().ToArray();
        if (shiftIds.Length != await db.WorkShifts.CountAsync(x => shiftIds.Contains(x.Id) && x.IsActive, ct))
            return Validation<WorkScheduleDetails>("Every interval must reference an active shift.");

        WorkSchedule entity;
        if (id is null) { entity = new WorkSchedule(); db.WorkSchedules.Add(entity); }
        else
        {
            entity = await db.WorkSchedules.Include(x => x.Days).ThenInclude(x => x.Intervals)
                .SingleOrDefaultAsync(x => x.Id == id, ct) ?? null!;
            if (entity is null) return NotFound<WorkScheduleDetails>();
            if (!SetVersion(entity, command.RowVersion)) return Validation<WorkScheduleDetails>("Row version is required.");
            db.WorkScheduleIntervals.RemoveRange(entity.Days.SelectMany(x => x.Intervals));
            db.WorkScheduleDays.RemoveRange(entity.Days); entity.Days.Clear();
        }
        Apply(entity, command);
        foreach (var day in command.Days)
        {
            var dayEntity = new WorkScheduleDay { DayNumber = day.DayNumber, TypeOfDay = day.TypeOfDay, Name = Clean(day.Name) };
            foreach (var interval in day.Intervals.OrderBy(x => x.SequenceNumber))
                dayEntity.Intervals.Add(new WorkScheduleInterval
                {
                    WorkShiftId = interval.WorkShiftId, SequenceNumber = interval.SequenceNumber,
                    StartTime = interval.StartTime, EndTime = interval.EndTime,
                    CrossesMidnight = interval.CrossesMidnight,
                    PaidMinutes = Minutes(interval.StartTime, interval.EndTime, interval.CrossesMidnight),
                    IsActive = interval.IsActive
                });
            entity.Days.Add(dayEntity);
        }
        var departments = await db.Departments.Where(x => x.WorkScheduleId == entity.Id || departmentIds.Contains(x.Id)).ToArrayAsync(ct);
        foreach (var department in departments)
            department.WorkScheduleId = departmentIds.Contains(department.Id) ? entity.Id : null;
        try
        {
            await db.SaveChangesAsync(ct);
            return SchedulingResult<WorkScheduleDetails>.Success((await GetScheduleAsync(entity.Id, ct))!);
        }
        catch (DbUpdateConcurrencyException) { return Conflict<WorkScheduleDetails>("The schedule was changed by another request."); }
        catch (DbUpdateException) { return Conflict<WorkScheduleDetails>("The schedule code, main status, day, or interval conflicts with existing data."); }
    }

    public async Task<SchedulingResult<WorkScheduleDetails>> AssignDepartmentAsync(Guid scheduleId, Guid departmentId, CancellationToken ct)
    {
        var schedule = await db.WorkSchedules.SingleOrDefaultAsync(x => x.Id == scheduleId, ct);
        if (schedule is null) return NotFound<WorkScheduleDetails>();
        if (schedule.Kind != WorkScheduleKind.Optional || schedule.Status != WorkScheduleStatus.Active)
            return Validation<WorkScheduleDetails>("Only an active optional schedule can be assigned to departments.");
        var department = await db.Departments.SingleOrDefaultAsync(x => x.Id == departmentId, ct);
        if (department is null) return NotFound<WorkScheduleDetails>();
        if (!department.IsActive) return Validation<WorkScheduleDetails>("The department must be active.");
        department.WorkScheduleId = scheduleId;
        await db.SaveChangesAsync(ct);
        return SchedulingResult<WorkScheduleDetails>.Success((await GetScheduleAsync(scheduleId, ct))!);
    }

    public async Task<SchedulingResult<WorkScheduleDetails>> RemoveDepartmentAsync(Guid scheduleId, Guid departmentId, CancellationToken ct)
    {
        var schedule = await db.WorkSchedules.SingleOrDefaultAsync(x => x.Id == scheduleId, ct);
        if (schedule is null) return NotFound<WorkScheduleDetails>();
        var department = await db.Departments.SingleOrDefaultAsync(x => x.Id == departmentId, ct);
        if (department is null) return NotFound<WorkScheduleDetails>();
        if (department.WorkScheduleId == scheduleId)
        {
            department.WorkScheduleId = null;
            await db.SaveChangesAsync(ct);
        }
        return SchedulingResult<WorkScheduleDetails>.Success((await GetScheduleAsync(scheduleId, ct))!);
    }

    public async Task<IReadOnlyCollection<WorkShiftModel>> GetShiftsAsync(CancellationToken ct) =>
        await db.WorkShifts.AsNoTracking().OrderBy(x => x.Code).Select(x => new WorkShiftModel(
            x.Id, x.Code, x.Name, x.Description, x.IsActive, Convert.ToBase64String(x.RowVersion))).ToArrayAsync(ct);

    public async Task<SchedulingResult<WorkShiftModel>> SaveShiftAsync(Guid? id, SaveWorkShiftCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Code) || string.IsNullOrWhiteSpace(command.Name))
            return Validation<WorkShiftModel>("Shift code and name are required.");
        WorkShift entity;
        if (id is null) { entity = new WorkShift(); db.WorkShifts.Add(entity); }
        else
        {
            entity = await db.WorkShifts.SingleOrDefaultAsync(x => x.Id == id, ct) ?? null!;
            if (entity is null) return NotFound<WorkShiftModel>();
            if (!SetVersion(entity, command.RowVersion)) return Validation<WorkShiftModel>("Row version is required.");
            if (!command.IsActive && await db.WorkScheduleIntervals.AnyAsync(x => x.WorkShiftId == id && x.IsActive, ct))
                return Dependency<WorkShiftModel>("A shift used by an active interval cannot be deactivated.");
        }
        entity.Code = command.Code.Trim().ToUpperInvariant(); entity.Name = command.Name.Trim();
        entity.Description = Clean(command.Description); entity.IsActive = command.IsActive;
        try { await db.SaveChangesAsync(ct); return SchedulingResult<WorkShiftModel>.Success(Shift(entity)); }
        catch (DbUpdateConcurrencyException) { return Conflict<WorkShiftModel>("The shift was changed by another request."); }
        catch (DbUpdateException) { return Conflict<WorkShiftModel>("A shift with this code already exists."); }
    }

    public async Task<IReadOnlyCollection<ProductionCalendarDetails>> GetCalendarsAsync(CancellationToken ct) =>
        (await db.ProductionCalendars.AsNoTracking().Include(x => x.Days).OrderByDescending(x => x.IsMain)
            .ThenBy(x => x.Code).ToArrayAsync(ct)).Select(Calendar).ToArray();

    public async Task<SchedulingResult<ProductionCalendarDetails>> SaveCalendarAsync(Guid? id, SaveProductionCalendarCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Code) || string.IsNullOrWhiteSpace(command.Name) ||
            command.CountryCode.Trim().Length != 2 || string.IsNullOrWhiteSpace(command.TimeZoneId))
            return Validation<ProductionCalendarDetails>("Code, name, two-letter country and time zone are required.");
        if (command.Days.GroupBy(x => x.Date).Any(x => x.Count() > 1))
            return Validation<ProductionCalendarDetails>("Only one calendar exception is allowed per date.");
        if (command.Days.Any(x => string.IsNullOrWhiteSpace(x.Name)))
            return Validation<ProductionCalendarDetails>("Every calendar exception needs a name.");
        if (command.IsMain && command.IsActive && await db.ProductionCalendars.AnyAsync(
                x => x.Id != id && x.IsMain && x.IsActive, ct))
            return Conflict<ProductionCalendarDetails>("Only one active main production calendar is allowed.");
        ProductionCalendar entity;
        if (id is null) { entity = new ProductionCalendar(); db.ProductionCalendars.Add(entity); }
        else
        {
            entity = await db.ProductionCalendars.Include(x => x.Days).SingleOrDefaultAsync(x => x.Id == id, ct) ?? null!;
            if (entity is null) return NotFound<ProductionCalendarDetails>();
            if (!SetVersion(entity, command.RowVersion)) return Validation<ProductionCalendarDetails>("Row version is required.");
            db.ProductionCalendarDays.RemoveRange(entity.Days); entity.Days.Clear();
        }
        entity.Code = command.Code.Trim().ToUpperInvariant(); entity.Name = command.Name.Trim();
        entity.CountryCode = command.CountryCode.Trim().ToUpperInvariant(); entity.RegionCode = Clean(command.RegionCode)?.ToUpperInvariant();
        entity.TimeZoneId = command.TimeZoneId.Trim(); entity.IsMain = command.IsMain; entity.IsActive = command.IsActive;
        foreach (var day in command.Days) entity.Days.Add(new ProductionCalendarDay
        { Date = day.Date, DayType = day.DayType, Name = day.Name.Trim(), TransferredFromDate = day.TransferredFromDate, Description = Clean(day.Description) });
        try { await db.SaveChangesAsync(ct); return SchedulingResult<ProductionCalendarDetails>.Success(Calendar(entity)); }
        catch (DbUpdateConcurrencyException) { return Conflict<ProductionCalendarDetails>("The calendar was changed by another request."); }
        catch (DbUpdateException) { return Conflict<ProductionCalendarDetails>("The calendar code, main status, or date conflicts with existing data."); }
    }

    public async Task<SchedulingResult<EffectiveScheduleDay>> GetEffectiveDayAsync(Guid? departmentId, DateOnly date, CancellationToken ct)
    {
        Guid? scheduleId = null;
        if (departmentId is not null)
        {
            var department = await db.Departments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == departmentId, ct);
            if (department is null) return NotFound<EffectiveScheduleDay>(); scheduleId = department.WorkScheduleId;
        }
        var schedule = await db.WorkSchedules.AsNoTracking().Include(x => x.Days).ThenInclude(x => x.Intervals)
            .ThenInclude(x => x.WorkShift).SingleOrDefaultAsync(x => x.Id == scheduleId && x.Status == WorkScheduleStatus.Active, ct)
            ?? await db.WorkSchedules.AsNoTracking().Include(x => x.Days).ThenInclude(x => x.Intervals)
                .ThenInclude(x => x.WorkShift).SingleOrDefaultAsync(x => x.Kind == WorkScheduleKind.Main && x.Status == WorkScheduleStatus.Active, ct);
        if (schedule is null) return NotFound<EffectiveScheduleDay>("No active work schedule is configured.");
        var exception = await db.ProductionCalendarDays.AsNoTracking().Where(x => x.Date == date && x.ProductionCalendar.IsMain && x.ProductionCalendar.IsActive)
            .Select(x => new { x.DayType, x.Name }).SingleOrDefaultAsync(ct);
        var type = ResolveType(exception?.DayType, date.DayOfWeek, schedule);
        var cycleDay = (date.DayNumber - schedule.CycleAnchorDate.DayNumber) % schedule.CycleLengthDays;
        if (cycleDay < 0) cycleDay += schedule.CycleLengthDays;
        var dayNumber = type == WorkScheduleDayType.Workday ? cycleDay + 1 : 0;
        var day = schedule.Days.SingleOrDefault(x => x.TypeOfDay == type && x.DayNumber == dayNumber)
            ?? schedule.Days.SingleOrDefault(x => x.TypeOfDay == WorkScheduleDayType.Workday && x.DayNumber == cycleDay + 1);
        var intervals = day?.Intervals.Where(x => x.IsActive).OrderBy(x => x.StartTime)
            .Select(Interval).ToArray() ?? [];
        return SchedulingResult<EffectiveScheduleDay>.Success(new(date, schedule.Id, schedule.Name,
            departmentId, type, exception?.Name, intervals));
    }

    private static string? Validate(SaveWorkScheduleCommand c)
    {
        if (string.IsNullOrWhiteSpace(c.Code) || string.IsNullOrWhiteSpace(c.Name) || string.IsNullOrWhiteSpace(c.TimeZoneId)) return "Code, name and time zone are required.";
        var expected = c.CycleType == WorkScheduleCycleType.Daily ? 1 : c.CycleType == WorkScheduleCycleType.Weekly ? 7 : c.CycleLengthDays;
        if (c.CycleLengthDays != expected || expected is < 1 or > 366) return "Cycle length does not match the selected cycle type.";
        if (!c.Days.Any(x => x.TypeOfDay == WorkScheduleDayType.Workday)) return "At least one workday is required.";
        if (c.Days.Where(x => x.TypeOfDay == WorkScheduleDayType.Workday).Any(x => x.DayNumber < 1 || x.DayNumber > expected)) return "Workday number is outside the cycle.";
        if (c.Days.Where(x => x.TypeOfDay != WorkScheduleDayType.Workday).Any(x => x.DayNumber != 0)) return "Special day templates must use day number zero.";
        if (c.Days.GroupBy(x => new { x.DayNumber, x.TypeOfDay }).Any(x => x.Count() > 1)) return "A day template is duplicated.";
        var required = new[] { (c.UsePreHolidayTemplate, WorkScheduleDayType.PreHoliday), (c.UseHolidayTemplate, WorkScheduleDayType.Holiday), (c.UseSaturdayTemplate, WorkScheduleDayType.Saturday), (c.UseSundayTemplate, WorkScheduleDayType.Sunday) };
        if (required.Any(x => x.Item1 && !c.Days.Any(d => d.TypeOfDay == x.Item2))) return "Every enabled special-day template must be defined.";
        foreach (var day in c.Days)
        {
            if (day.Intervals.GroupBy(x => new { x.WorkShiftId, x.SequenceNumber }).Any(x => x.Count() > 1)) return "Interval sequence is duplicated within a shift.";
            var ranges = day.Intervals.Where(x => x.IsActive).SelectMany(Ranges).OrderBy(x => x.Start).ToArray();
            if (ranges.Zip(ranges.Skip(1)).Any(x => x.First.End > x.Second.Start)) return "Work intervals overlap within a day.";
            if (day.Intervals.Any(x => Minutes(x.StartTime, x.EndTime, x.CrossesMidnight) <= 0)) return "Every interval must have a positive duration.";
        }
        return null;
    }

    private static IEnumerable<(int Start, int End)> Ranges(WorkScheduleIntervalModel x)
    {
        var start = x.StartTime.Hour * 60 + x.StartTime.Minute; var end = x.EndTime.Hour * 60 + x.EndTime.Minute;
        if (!x.CrossesMidnight) { yield return (start, end); yield break; }
        yield return (start, 1440); if (end > 0) yield return (0, end);
    }
    private static int Minutes(TimeOnly start, TimeOnly end, bool crosses) =>
        (end.Hour * 60 + end.Minute) - (start.Hour * 60 + start.Minute) + (crosses ? 1440 : 0);
    private bool SetVersion(AuditableEntity entity, string? version) { try { if (string.IsNullOrWhiteSpace(version)) return false; db.Entry(entity).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(version); return true; } catch (FormatException) { return false; } }
    private static void Apply(WorkSchedule x, SaveWorkScheduleCommand c) { x.Code = c.Code.Trim().ToUpperInvariant(); x.Name = c.Name.Trim(); x.Description = Clean(c.Description); x.Kind = c.Kind; x.Status = c.Status; x.TimeZoneId = c.TimeZoneId.Trim(); x.CycleType = c.CycleType; x.CycleLengthDays = c.CycleLengthDays; x.CycleAnchorDate = c.CycleAnchorDate; x.UsePreHolidayTemplate = c.UsePreHolidayTemplate; x.UseHolidayTemplate = c.UseHolidayTemplate; x.UseSaturdayTemplate = c.UseSaturdayTemplate; x.UseSundayTemplate = c.UseSundayTemplate; }
    private static WorkScheduleDetails Details(WorkSchedule x) => new(x.Id, x.Code, x.Name, x.Description, x.Kind, x.Status, x.TimeZoneId, x.CycleType, x.CycleLengthDays, x.CycleAnchorDate, x.UsePreHolidayTemplate, x.UseHolidayTemplate, x.UseSaturdayTemplate, x.UseSundayTemplate, x.Departments.Select(d => d.Id).ToArray(), x.Days.OrderBy(d => d.TypeOfDay).ThenBy(d => d.DayNumber).Select(d => new WorkScheduleDayModel(d.Id, d.DayNumber, d.TypeOfDay, d.Name, d.Intervals.OrderBy(i => i.WorkShift.Code).ThenBy(i => i.SequenceNumber).Select(Interval).ToArray(), Convert.ToBase64String(d.RowVersion))).ToArray(), Convert.ToBase64String(x.RowVersion));
    private static WorkScheduleIntervalModel Interval(WorkScheduleInterval x) => new(x.Id, x.WorkShiftId, x.WorkShift.Code, x.WorkShift.Name, x.SequenceNumber, x.StartTime, x.EndTime, x.PaidMinutes, x.CrossesMidnight, x.IsActive, Convert.ToBase64String(x.RowVersion));
    private static WorkShiftModel Shift(WorkShift x) => new(x.Id, x.Code, x.Name, x.Description, x.IsActive, Convert.ToBase64String(x.RowVersion));
    private static ProductionCalendarDetails Calendar(ProductionCalendar x) => new(x.Id, x.Code, x.Name, x.CountryCode, x.RegionCode, x.TimeZoneId, x.IsMain, x.IsActive, x.Days.OrderBy(d => d.Date).Select(d => new ProductionCalendarDayModel(d.Id, d.Date, d.DayType, d.Name, d.TransferredFromDate, d.Description, Convert.ToBase64String(d.RowVersion))).ToArray(), Convert.ToBase64String(x.RowVersion));
    private static WorkScheduleDayType ResolveType(ProductionCalendarDayType? exception, DayOfWeek day, WorkSchedule schedule) => exception switch { ProductionCalendarDayType.Holiday or ProductionCalendarDayType.TransferredDayOff or ProductionCalendarDayType.SpecialDayOff when schedule.UseHolidayTemplate => WorkScheduleDayType.Holiday, ProductionCalendarDayType.PreHoliday when schedule.UsePreHolidayTemplate => WorkScheduleDayType.PreHoliday, _ when day == DayOfWeek.Saturday && schedule.UseSaturdayTemplate => WorkScheduleDayType.Saturday, _ when day == DayOfWeek.Sunday && schedule.UseSundayTemplate => WorkScheduleDayType.Sunday, _ => WorkScheduleDayType.Workday };
    private static string? Clean(string? x) => string.IsNullOrWhiteSpace(x) ? null : x.Trim();
    private static SchedulingResult<T> Validation<T>(string x) => SchedulingResult<T>.Failure(SchedulingError.Validation, x);
    private static SchedulingResult<T> Conflict<T>(string x) => SchedulingResult<T>.Failure(SchedulingError.Conflict, x);
    private static SchedulingResult<T> Dependency<T>(string x) => SchedulingResult<T>.Failure(SchedulingError.Dependency, x);
    private static SchedulingResult<T> NotFound<T>(string x = "Record was not found.") => SchedulingResult<T>.Failure(SchedulingError.NotFound, x);
}
