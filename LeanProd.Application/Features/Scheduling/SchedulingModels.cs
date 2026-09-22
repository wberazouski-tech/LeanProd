using LeanProd.Domain.Scheduling;

namespace LeanProd.Application.Features.Scheduling;

public sealed record WorkScheduleSummary(Guid Id, string Code, string Name, WorkScheduleKind Kind,
    WorkScheduleStatus Status, WorkScheduleCycleType CycleType, int CycleLengthDays, int DepartmentCount);
public sealed record WorkScheduleIntervalModel(Guid? Id, Guid WorkShiftId, string? WorkShiftCode,
    string? WorkShiftName, int SequenceNumber, TimeOnly StartTime, TimeOnly EndTime,
    int PaidMinutes, bool CrossesMidnight, bool IsActive, string? RowVersion);
public sealed record WorkScheduleDayModel(Guid? Id, int DayNumber, WorkScheduleDayType TypeOfDay,
    string? Name, IReadOnlyCollection<WorkScheduleIntervalModel> Intervals, string? RowVersion);
public sealed record WorkScheduleDetails(Guid Id, string Code, string Name, string? Description,
    WorkScheduleKind Kind, WorkScheduleStatus Status, string TimeZoneId,
    WorkScheduleCycleType CycleType, int CycleLengthDays, DateOnly CycleAnchorDate,
    bool UsePreHolidayTemplate, bool UseHolidayTemplate, bool UseSaturdayTemplate,
    bool UseSundayTemplate, IReadOnlyCollection<Guid> DepartmentIds,
    IReadOnlyCollection<WorkScheduleDayModel> Days, string RowVersion);
public sealed record SaveWorkScheduleCommand(string Code, string Name, string? Description,
    WorkScheduleKind Kind, WorkScheduleStatus Status, string TimeZoneId,
    WorkScheduleCycleType CycleType, int CycleLengthDays, DateOnly CycleAnchorDate,
    bool UsePreHolidayTemplate, bool UseHolidayTemplate, bool UseSaturdayTemplate,
    bool UseSundayTemplate, IReadOnlyCollection<Guid> DepartmentIds,
    IReadOnlyCollection<WorkScheduleDayModel> Days, string? RowVersion);

public sealed record WorkShiftModel(Guid Id, string Code, string Name, string? Description,
    bool IsActive, string RowVersion);
public sealed record SaveWorkShiftCommand(string Code, string Name, string? Description,
    bool IsActive, string? RowVersion);

public sealed record ProductionCalendarDayModel(Guid? Id, DateOnly Date,
    ProductionCalendarDayType DayType, string Name, DateOnly? TransferredFromDate,
    string? Description, string? RowVersion);
public sealed record ProductionCalendarDetails(Guid Id, string Code, string Name, string CountryCode,
    string? RegionCode, string TimeZoneId, bool IsMain, bool IsActive,
    IReadOnlyCollection<ProductionCalendarDayModel> Days, string RowVersion);
public sealed record SaveProductionCalendarCommand(string Code, string Name, string CountryCode,
    string? RegionCode, string TimeZoneId, bool IsMain, bool IsActive,
    IReadOnlyCollection<ProductionCalendarDayModel> Days, string? RowVersion);
public sealed record EffectiveScheduleDay(DateOnly Date, Guid WorkScheduleId, string WorkScheduleName,
    Guid? DepartmentId, WorkScheduleDayType TypeOfDay, string? CalendarDayName,
    IReadOnlyCollection<WorkScheduleIntervalModel> Intervals);

public enum SchedulingError { None, NotFound, Validation, Conflict, Dependency }
public sealed record SchedulingResult<T>(T? Value, SchedulingError Error, string? Message = null)
{
    public bool Succeeded => Error == SchedulingError.None;
    public static SchedulingResult<T> Success(T value) => new(value, SchedulingError.None);
    public static SchedulingResult<T> Failure(SchedulingError error, string message) => new(default, error, message);
}
