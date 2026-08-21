using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Api.Features.MasterData.Contracts;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Features.MasterData;

[ApiController, Route("api/equipment"), Authorize(Policy = Permissions.MasterDataView)]
public sealed class EquipmentController(IEquipmentService service) : MasterDataControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MasterDataPage<EquipmentSummary>>> List(int page = 1, int pageSize = 20,
        string? search = null, bool? isActive = null, Guid? departmentId = null, Guid? equipmentTypeId = null,
        string? state = null, CancellationToken ct = default)
    {
        if (page < 1 || pageSize is < 1 or > 100) return Problem(statusCode: 400, title: "Invalid paging");
        return Ok(await service.GetEquipmentAsync(new EquipmentQuery(page, pageSize, search, isActive, departmentId, equipmentTypeId, state), ct));
    }

    [HttpGet("options")]
    public async Task<ActionResult<IReadOnlyCollection<EquipmentOption>>> Options(CancellationToken ct) => Ok(await service.GetEquipmentOptionsAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EquipmentDetails>> Get(Guid id, CancellationToken ct)
    { var value = await service.GetEquipmentAsync(id, ct); return value is null ? NotFound() : Ok(value); }

    [HttpPost, Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<EquipmentDetails>> Create(SaveEquipmentRequest request, CancellationToken ct)
    { var result = await service.CreateEquipmentAsync(Command(request), ct); return result.Succeeded ? CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value) : Map(result); }

    [HttpPut("{id:guid}"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<EquipmentDetails>> Update(Guid id, SaveEquipmentRequest request, CancellationToken ct) => Map(await service.UpdateEquipmentAsync(id, Command(request), ct));

    [HttpPost("{id:guid}/activate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Activate(Guid id, CancellationToken ct) => Map(await service.SetEquipmentActiveAsync(id, true, ct));

    [HttpPost("{id:guid}/deactivate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Deactivate(Guid id, CancellationToken ct) => Map(await service.SetEquipmentActiveAsync(id, false, ct));

    [HttpGet("{id:guid}/states")]
    public async Task<ActionResult<IReadOnlyCollection<EquipmentStateEventDetails>>> States(Guid id, CancellationToken ct) => Ok(await service.GetStateHistoryAsync(id, ct));

    [HttpPost("{id:guid}/states"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<EquipmentStateEventDetails>> ChangeState(Guid id, ChangeEquipmentStateRequest request, CancellationToken ct) =>
        Map(await service.ChangeStateAsync(id, new(request.State, request.StartedAtUtc, request.EndedAtUtc, request.Comment, request.RowVersion), ct));

    [HttpPut("{id:guid}/states/{eventId:guid}"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<EquipmentStateEventDetails>> UpdateState(Guid id, Guid eventId, ChangeEquipmentStateRequest request, CancellationToken ct) =>
        Map(await service.UpdateStateAsync(id, eventId, new(request.State, request.StartedAtUtc, request.EndedAtUtc, request.Comment, request.RowVersion), ct));

    private static SaveEquipmentCommand Command(SaveEquipmentRequest x) => new(x.Name, x.InventoryNumber,
        x.EquipmentTypeId, x.DepartmentId, x.ParentEquipmentId, x.SerialNumber, x.Manufacturer, x.Model,
        x.CommissionedOn, x.Description, x.RowVersion);
}
