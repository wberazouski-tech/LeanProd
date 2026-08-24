using System;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Api.Features.MasterData.Contracts;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.MasterData;
using LeanProd.Domain.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Features.MasterData;

[ApiController, Route("api/catalog-items"), Authorize(Policy = Permissions.MasterDataView)]
public sealed class CatalogItemsController(ICatalogItemService service) : MasterDataControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MasterDataPage<CatalogItemSummary>>> List(
        CatalogItemType type, int page = 1, int pageSize = 100,
        string? search = null, bool? isActive = null, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(type)) return Problem(statusCode: 400, title: "Invalid item type");
        if (page < 1 || pageSize is < 1 or > 5000) return Problem(statusCode: 400, title: "Invalid paging");
        return Ok(await service.GetItemsAsync(type, new(page, pageSize, search, isActive), ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CatalogItemDetails>> Get(Guid id, CancellationToken ct)
    {
        var item = await service.GetItemAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost, Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<CatalogItemDetails>> Create(SaveCatalogItemRequest request, CancellationToken ct)
    {
        var result = await service.CreateItemAsync(Command(request), ct);
        return result.Succeeded
            ? CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value)
            : Map(result);
    }

    [HttpPut("{id:guid}"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<CatalogItemDetails>> Update(Guid id, SaveCatalogItemRequest request, CancellationToken ct) =>
        Map(await service.UpdateItemAsync(id, Command(request), ct));

    [HttpPatch("{id:guid}/class"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<CatalogItemDetails>> ChangeClass(
        Guid id, ChangeCatalogItemClassRequest request, CancellationToken ct) =>
        Map(await service.ChangeItemClassAsync(id, request.CatalogItemClassId, ct));

    [HttpPost("{id:guid}/activate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Activate(Guid id, CancellationToken ct) =>
        Map(await service.SetItemActiveAsync(id, true, ct));

    [HttpPost("{id:guid}/deactivate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Deactivate(Guid id, CancellationToken ct) =>
        Map(await service.SetItemActiveAsync(id, false, ct));

    private static SaveCatalogItemCommand Command(SaveCatalogItemRequest request) => new(
        request.WorkingName, request.FullName, request.ArticleNumber, request.Type,
        request.BaseUnitOfMeasureId, request.CatalogItemClassId, request.Cost,
        request.Description, request.RowVersion);
}
