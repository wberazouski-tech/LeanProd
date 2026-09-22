using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace LeanProd.Api.Features.Scheduling;
[ApiController, Route("api/work-shifts"), Authorize(Policy = Permissions.WorkSchedulesView)]
public sealed class WorkShiftsController(ISchedulingService service) : SchedulingControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyCollection<WorkShiftModel>>> List(CancellationToken ct) => Ok(await service.GetShiftsAsync(ct));
    [HttpPost, Authorize(Policy = Permissions.WorkSchedulesManage)] public async Task<ActionResult<WorkShiftModel>> Create(SaveWorkShiftCommand command, CancellationToken ct) => Map(await service.SaveShiftAsync(null, command, ct));
    [HttpPut("{id:guid}"), Authorize(Policy = Permissions.WorkSchedulesManage)] public async Task<ActionResult<WorkShiftModel>> Update(Guid id, SaveWorkShiftCommand command, CancellationToken ct) => Map(await service.SaveShiftAsync(id, command, ct));
}

