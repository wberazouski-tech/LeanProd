using LeanProd.Domain.Common;
using LeanProd.Domain.MasterData;

namespace LeanProd.Domain.Scheduling;

public enum WorkScheduleKind { Main, Optional }
public enum WorkScheduleStatus { Draft, Active, Archived }
public enum WorkScheduleCycleType { Daily, Weekly, Custom }
public enum WorkScheduleDayType { Workday, PreHoliday, Holiday, Saturday, Sunday }
public enum ProductionCalendarDayType { Holiday, PreHoliday, TransferredWorkingDay, TransferredDayOff, SpecialWorkingDay, SpecialDayOff }

public sealed class WorkSchedule : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkScheduleKind Kind { get; set; }
    public WorkScheduleStatus Status { get; set; } = WorkScheduleStatus.Draft;
    public string TimeZoneId { get; set; } = "Europe/Warsaw";
    public WorkScheduleCycleType CycleType { get; set; } = WorkScheduleCycleType.Weekly;
    public int CycleLengthDays { get; set; } = 7;
    public DateOnly CycleAnchorDate { get; set; } = new(2024, 1, 1);
    public bool UsePreHolidayTemplate { get; set; }
    public bool UseHolidayTemplate { get; set; }
    public bool UseSaturdayTemplate { get; set; }
    public bool UseSundayTemplate { get; set; }
    public List<WorkScheduleDay> Days { get; set; } = [];
    public List<Department> Departments { get; set; } = [];
}

public sealed class WorkShift : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<WorkScheduleInterval> Intervals { get; set; } = [];
}

public sealed class WorkScheduleDay : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkScheduleId { get; set; }
    public WorkSchedule WorkSchedule { get; set; } = null!;
    public int DayNumber { get; set; }
    public WorkScheduleDayType TypeOfDay { get; set; }
    public string? Name { get; set; }
    public List<WorkScheduleInterval> Intervals { get; set; } = [];
}

public sealed class WorkScheduleInterval : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkScheduleDayId { get; set; }
    public WorkScheduleDay WorkScheduleDay { get; set; } = null!;
    public Guid WorkShiftId { get; set; }
    public WorkShift WorkShift { get; set; } = null!;
    public int SequenceNumber { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int PaidMinutes { get; set; }
    public bool CrossesMidnight { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ProductionCalendar : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "PL";
    public string? RegionCode { get; set; }
    public string TimeZoneId { get; set; } = "Europe/Warsaw";
    public bool IsMain { get; set; }
    public bool IsActive { get; set; } = true;
    public List<ProductionCalendarDay> Days { get; set; } = [];
}

public sealed class ProductionCalendarDay : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductionCalendarId { get; set; }
    public ProductionCalendar ProductionCalendar { get; set; } = null!;
    public DateOnly Date { get; set; }
    public ProductionCalendarDayType DayType { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly? TransferredFromDate { get; set; }
    public string? Description { get; set; }
}
