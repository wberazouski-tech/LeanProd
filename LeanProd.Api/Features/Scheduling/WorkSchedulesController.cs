using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace LeanProd.Api.Features.Scheduling;
[ApiController, Route("api/work-schedules"), Authorize(Policy = Permissions.WorkSchedulesView)]
public sealed class WorkSchedulesController(ISchedulingService service) : SchedulingControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyCollection<WorkScheduleSummary>>> List(CancellationToken ct) => Ok(await service.GetSchedulesAsync(ct));
    [HttpGet("{id:guid}")] public async Task<ActionResult<WorkScheduleDetails>> Get(Guid id, CancellationToken ct) { var item = await service.GetScheduleAsync(id, ct); return item is null ? NotFound() : Ok(item); }
    [HttpPost, Authorize(Policy = Permissions.WorkSchedulesManage)] public async Task<ActionResult<WorkScheduleDetails>> Create(SaveWorkScheduleCommand command, CancellationToken ct) => Map(await service.SaveScheduleAsync(null, command, ct));
    [HttpPut("{id:guid}"), Authorize(Policy = Permissions.WorkSchedulesManage)] public async Task<ActionResult<WorkScheduleDetails>> Update(Guid id, SaveWorkScheduleCommand command, CancellationToken ct) => Map(await service.SaveScheduleAsync(id, command, ct));
    [HttpPost("{id:guid}/departments/{departmentId:guid}"), Authorize(Policy = Permissions.WorkSchedulesManage)]
    public async Task<ActionResult<WorkScheduleDetails>> AssignDepartment(Guid id, Guid departmentId, CancellationToken ct) => Map(await service.AssignDepartmentAsync(id, departmentId, ct));
    [HttpDelete("{id:guid}/departments/{departmentId:guid}"), Authorize(Policy = Permissions.WorkSchedulesManage)]
    public async Task<ActionResult<WorkScheduleDetails>> RemoveDepartment(Guid id, Guid departmentId, CancellationToken ct) => Map(await service.RemoveDepartmentAsync(id, departmentId, ct));
    [HttpGet("effective")] public async Task<ActionResult<EffectiveScheduleDay>> Effective([FromQuery] Guid? departmentId, [FromQuery] DateOnly date, CancellationToken ct) => Map(await service.GetEffectiveDayAsync(departmentId, date, ct));
}

