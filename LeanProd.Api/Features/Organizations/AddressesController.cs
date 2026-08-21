using System;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Api.Features.MasterData;
using LeanProd.Api.Features.Organizations.Contracts;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Features.Organizations;

[ApiController, Route("api/addresses"), Authorize(Policy = Permissions.MasterDataManage)]
public sealed class AddressesController(IAddressService service) : MasterDataControllerBase
{
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AddressDetails>> Update(Guid id, SaveAddressRequest x, CancellationToken ct) =>
        Map(await service.UpdateAsync(id, OrganizationController.Command(x), ct));
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<bool>> Activate(Guid id, CancellationToken ct) => Map(await service.SetActiveAsync(id, true, ct));
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<bool>> Deactivate(Guid id, CancellationToken ct) => Map(await service.SetActiveAsync(id, false, ct));
    [HttpPost("{id:guid}/make-primary")]
    public async Task<ActionResult<bool>> MakePrimary(Guid id, CancellationToken ct) => Map(await service.MakePrimaryAsync(id, ct));
}
