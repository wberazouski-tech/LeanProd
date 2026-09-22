namespace LeanProd.Application.Features.Scheduling;

public interface ISchedulingService
{
    Task<IReadOnlyCollection<WorkScheduleSummary>> GetSchedulesAsync(CancellationToken ct);
    Task<WorkScheduleDetails?> GetScheduleAsync(Guid id, CancellationToken ct);
    Task<SchedulingResult<WorkScheduleDetails>> SaveScheduleAsync(Guid? id, SaveWorkScheduleCommand command, CancellationToken ct);
    Task<SchedulingResult<WorkScheduleDetails>> AssignDepartmentAsync(Guid scheduleId, Guid departmentId, CancellationToken ct);
    Task<SchedulingResult<WorkScheduleDetails>> RemoveDepartmentAsync(Guid scheduleId, Guid departmentId, CancellationToken ct);
    Task<IReadOnlyCollection<WorkShiftModel>> GetShiftsAsync(CancellationToken ct);
    Task<SchedulingResult<WorkShiftModel>> SaveShiftAsync(Guid? id, SaveWorkShiftCommand command, CancellationToken ct);
    Task<IReadOnlyCollection<ProductionCalendarDetails>> GetCalendarsAsync(CancellationToken ct);
    Task<SchedulingResult<ProductionCalendarDetails>> SaveCalendarAsync(Guid? id, SaveProductionCalendarCommand command, CancellationToken ct);
    Task<SchedulingResult<EffectiveScheduleDay>> GetEffectiveDayAsync(Guid? departmentId, DateOnly date, CancellationToken ct);
}
