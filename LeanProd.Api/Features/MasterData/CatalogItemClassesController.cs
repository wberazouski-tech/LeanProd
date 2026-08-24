using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Api.Features.MasterData.Contracts;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.MasterData;
using LeanProd.Domain.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Features.MasterData;

[ApiController, Route("api/catalog-item-classes"), Authorize(Policy = Permissions.MasterDataView)]
public sealed class CatalogItemClassesController(IMasterDataService service) : MasterDataControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MasterDataPage<CatalogItemClassSummary>>> List(
        CatalogItemType type, int page = 1, int pageSize = 100,
        string? search = null, bool? isActive = null, bool? isGroup = null,
        CancellationToken ct = default)
    {
        if (!Enum.IsDefined(type)) return Problem(statusCode: 400, title: "Invalid item type");
        if (page < 1 || pageSize is < 1 or > 5000) return Problem(statusCode: 400, title: "Invalid paging");
        return Ok(await service.GetCatalogItemClassesAsync(type, new(page, pageSize, search, isActive), isGroup, ct));
    }

    [HttpGet("options")]
    public async Task<ActionResult<IReadOnlyCollection<CatalogItemClassOption>>> Options(
        CatalogItemType type, bool activeOnly = true, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(type)) return Problem(statusCode: 400, title: "Invalid item type");
        return Ok(await service.GetCatalogItemClassOptionsAsync(type, activeOnly, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CatalogItemClassDetails>> Get(Guid id, CancellationToken ct)
    {
        var value = await service.GetCatalogItemClassAsync(id, ct);
        return value is null ? NotFound() : Ok(value);
    }

    [HttpPost, Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<CatalogItemClassDetails>> Create(
        SaveCatalogItemClassRequest request, CancellationToken ct)
    {
        var result = await service.CreateCatalogItemClassAsync(Command(request), ct);
        return result.Succeeded
            ? CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value)
            : Map(result);
    }

    [HttpPut("{id:guid}"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<CatalogItemClassDetails>> Update(
        Guid id, SaveCatalogItemClassRequest request, CancellationToken ct) =>
        Map(await service.UpdateCatalogItemClassAsync(id, Command(request), ct));

    [HttpPost("{id:guid}/activate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Activate(Guid id, CancellationToken ct) =>
        Map(await service.SetCatalogItemClassActiveAsync(id, true, ct));

    [HttpPost("{id:guid}/deactivate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Deactivate(Guid id, CancellationToken ct) =>
        Map(await service.SetCatalogItemClassActiveAsync(id, false, ct));

    private static SaveCatalogItemClassCommand Command(SaveCatalogItemClassRequest request) =>
        new(request.Type, request.Code, request.Name, request.IsGroup, request.ParentId, request.RowVersion);
}
