using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Api.Features.MasterData.Contracts;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.MasterData;
using LeanProd.Application.Features.Organizations;
using LeanProd.Api.Features.Organizations;
using LeanProd.Api.Features.Organizations.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Features.MasterData;

[ApiController, Route("api/departments"), Authorize(Policy = Permissions.MasterDataView)]
public sealed class DepartmentsController(IDepartmentService service, IAddressService addresses) : MasterDataControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MasterDataPage<DepartmentSummary>>> List(int page = 1, int pageSize = 20, string? search = null, bool? isActive = null, CancellationToken ct = default)
    {
        if (page < 1 || pageSize is < 1 or > 5000) return Problem(statusCode: 400, title: "Invalid paging");
        return Ok(await service.GetDepartmentsAsync(new(page, pageSize, search, isActive), ct));
    }
    [HttpGet("options")]
    public async Task<ActionResult<IReadOnlyCollection<OptionItem>>> Options(CancellationToken ct) => Ok(await service.GetDepartmentOptionsAsync(ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DepartmentDetails>> Get(Guid id, CancellationToken ct) { var value = await service.GetDepartmentAsync(id, ct); return value is null ? NotFound() : Ok(value); }
    [HttpPost, Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<DepartmentDetails>> Create(SaveDepartmentRequest request, CancellationToken ct)
    { var result = await service.CreateDepartmentAsync(Command(request), ct); return result.Succeeded ? CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value) : Map(result); }
    [HttpPut("{id:guid}"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<DepartmentDetails>> Update(Guid id, SaveDepartmentRequest request, CancellationToken ct) => Map(await service.UpdateDepartmentAsync(id, Command(request), ct));
    [HttpPost("{id:guid}/activate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Activate(Guid id, CancellationToken ct) => Map(await service.SetDepartmentActiveAsync(id, true, ct));
    [HttpPost("{id:guid}/deactivate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Deactivate(Guid id, CancellationToken ct) => Map(await service.SetDepartmentActiveAsync(id, false, ct));
    [HttpGet("{id:guid}/addresses")]
    public async Task<ActionResult<IReadOnlyCollection<AddressDetails>>> Addresses(Guid id, CancellationToken ct) => Ok(await addresses.GetAsync(new(AddressOwnerType.Department, id), ct));
    [HttpPost("{id:guid}/addresses"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<AddressDetails>> CreateAddress(Guid id, SaveAddressRequest x, CancellationToken ct) => Map(await addresses.CreateAsync(new(AddressOwnerType.Department, id), OrganizationController.Command(x), ct));
    private static SaveDepartmentCommand Command(SaveDepartmentRequest x) => new(x.Code, x.Name, x.Description, x.ParentDepartmentId, x.RowVersion);
}
