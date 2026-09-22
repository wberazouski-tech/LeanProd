using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace LeanProd.Api.Features.Scheduling;
[ApiController, Route("api/production-calendars"), Authorize(Policy = Permissions.WorkSchedulesView)]
public sealed class ProductionCalendarsController(ISchedulingService service) : SchedulingControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyCollection<ProductionCalendarDetails>>> List(CancellationToken ct) => Ok(await service.GetCalendarsAsync(ct));
    [HttpPost, Authorize(Policy = Permissions.WorkSchedulesManage)] public async Task<ActionResult<ProductionCalendarDetails>> Create(SaveProductionCalendarCommand command, CancellationToken ct) => Map(await service.SaveCalendarAsync(null, command, ct));
    [HttpPut("{id:guid}"), Authorize(Policy = Permissions.WorkSchedulesManage)] public async Task<ActionResult<ProductionCalendarDetails>> Update(Guid id, SaveProductionCalendarCommand command, CancellationToken ct) => Map(await service.SaveCalendarAsync(id, command, ct));
}



