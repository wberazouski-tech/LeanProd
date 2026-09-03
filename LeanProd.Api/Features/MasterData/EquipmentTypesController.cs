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

[ApiController, Route("api/equipment-types"), Authorize(Policy = Permissions.MasterDataView)]
public sealed class EquipmentTypesController(IEquipmentService service) : MasterDataControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EquipmentTypeDetails>>> List(bool activeOnly = false, CancellationToken ct = default) => Ok(await service.GetEquipmentTypesAsync(activeOnly, ct));

    [HttpPost, Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<EquipmentTypeDetails>> Create(SaveEquipmentTypeRequest request, CancellationToken ct) =>
        Map(await service.CreateEquipmentTypeAsync(new(request.Name, request.Description, request.IsActive ?? true, request.RowVersion), ct));

    [HttpPut("{id:guid}"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<EquipmentTypeDetails>> Update(Guid id, SaveEquipmentTypeRequest request, CancellationToken ct) =>
        Map(await service.UpdateEquipmentTypeAsync(id, new(request.Name, request.Description, request.IsActive ?? true, request.RowVersion), ct));

    [HttpPost("{id:guid}/activate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Activate(Guid id, CancellationToken ct) => Map(await service.SetEquipmentTypeActiveAsync(id, true, ct));

    [HttpPost("{id:guid}/deactivate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Deactivate(Guid id, CancellationToken ct) => Map(await service.SetEquipmentTypeActiveAsync(id, false, ct));
}
