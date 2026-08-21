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

[ApiController, Route("api/storage-locations"), Authorize(Policy = Permissions.MasterDataView)]
public sealed class StorageLocationsController(IMasterDataService service, IAddressService addresses) : MasterDataControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MasterDataPage<StorageLocationSummary>>> List(int page = 1, int pageSize = 20, string? search = null, bool? isActive = null, Guid? departmentId = null, Guid? kindId = null, Guid? typeId = null, CancellationToken ct = default)
    {
        if (page < 1 || pageSize is < 1 or > 5000) return Problem(statusCode: 400, title: "Invalid paging");
        return Ok(await service.GetStorageLocationsAsync(new(page, pageSize, search, isActive), departmentId, kindId, typeId, ct));
    }
    [HttpGet("options")]
    public async Task<ActionResult<IReadOnlyCollection<StorageLocationOption>>> Options(CancellationToken ct) => Ok(await service.GetStorageLocationOptionsAsync(ct));
    [HttpGet("kinds")]
    public async Task<ActionResult<IReadOnlyCollection<CatalogItem>>> Kinds(CancellationToken ct) => Ok(await service.GetStorageLocationKindsAsync(ct));
    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyCollection<CatalogItem>>> Types(CancellationToken ct) => Ok(await service.GetStorageLocationTypesAsync(ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StorageLocationDetails>> Get(Guid id, CancellationToken ct) { var value = await service.GetStorageLocationAsync(id, ct); return value is null ? NotFound() : Ok(value); }
    [HttpPost, Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<StorageLocationDetails>> Create(SaveStorageLocationRequest request, CancellationToken ct)
    { var result = await service.CreateStorageLocationAsync(Command(request), ct); return result.Succeeded ? CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value) : Map(result); }
    [HttpPut("{id:guid}"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<StorageLocationDetails>> Update(Guid id, SaveStorageLocationRequest request, CancellationToken ct) => Map(await service.UpdateStorageLocationAsync(id, Command(request), ct));
    [HttpPost("{id:guid}/activate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Activate(Guid id, CancellationToken ct) => Map(await service.SetStorageLocationActiveAsync(id, true, ct));
    [HttpPost("{id:guid}/deactivate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Deactivate(Guid id, CancellationToken ct) => Map(await service.SetStorageLocationActiveAsync(id, false, ct));
    [HttpGet("{id:guid}/addresses")]
    public async Task<ActionResult<IReadOnlyCollection<AddressDetails>>> Addresses(Guid id, CancellationToken ct) => Ok(await addresses.GetAsync(new(AddressOwnerType.StorageLocation, id), ct));
    [HttpPost("{id:guid}/addresses"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<AddressDetails>> CreateAddress(Guid id, SaveAddressRequest x, CancellationToken ct) => Map(await addresses.CreateAsync(new(AddressOwnerType.StorageLocation, id), OrganizationController.Command(x), ct));
    private static SaveStorageLocationCommand Command(SaveStorageLocationRequest x) => new(x.Code, x.Name, x.Description, x.DepartmentId, x.KindId, x.ParentStorageLocationId, x.TypeIds, x.RowVersion);
}
