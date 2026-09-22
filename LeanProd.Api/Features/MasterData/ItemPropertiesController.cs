using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Features.MasterData;

[ApiController, Authorize(Policy = Permissions.MasterDataView)]
public sealed class ItemPropertiesController(IItemPropertyService service) : MasterDataControllerBase
{
    [HttpGet("api/catalog-item-classes/{classId:guid}/properties")]
    public async Task<ActionResult<IReadOnlyList<PropertyDefinitionDto>>> Definitions(Guid classId, CancellationToken ct) =>
        Map(await service.Definitions(classId, ct));

    [HttpPost("api/catalog-item-classes/{classId:guid}/properties"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<PropertyDefinitionDto>> CreateDefinition(Guid classId, SavePropertyCommand command, CancellationToken ct) =>
        Map(await service.SaveDefinition(classId, null, command, ct));

    [HttpPut("api/catalog-item-classes/{classId:guid}/properties/{id:guid}"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<PropertyDefinitionDto>> UpdateDefinition(Guid classId, Guid id, SavePropertyCommand command, CancellationToken ct) =>
        Map(await service.SaveDefinition(classId, id, command, ct));

    [HttpGet("api/catalog-items/{itemId:guid}/properties")]
    [HttpGet("api/catalog-items/{itemId:guid}/batches/{batchId:guid}/properties")]
    public async Task<ActionResult<PropertyValuesDto>> Values(Guid itemId, Guid? batchId, CancellationToken ct) =>
        Map(await service.Values(itemId, batchId, ct));

    [HttpPut("api/catalog-items/{itemId:guid}/properties"), Authorize(Policy = Permissions.MasterDataManage)]
    [HttpPut("api/catalog-items/{itemId:guid}/batches/{batchId:guid}/properties")]
    public async Task<ActionResult<PropertyValuesDto>> SaveValues(Guid itemId, Guid? batchId, SavePropertyValuesCommand command, CancellationToken ct) =>
        Map(await service.SaveValues(itemId, batchId, command, ct));

    [HttpGet("api/catalog-items/{itemId:guid}/batches")]
    public async Task<ActionResult<MasterDataPage<BatchDto>>> Batches(Guid itemId, CancellationToken ct, [FromQuery] int page = 1) =>
        Map(await service.Batches(itemId, page, ct));

    [HttpPost("api/catalog-items/{itemId:guid}/batches"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<BatchDto>> CreateBatch(Guid itemId, SaveBatchCommand command, CancellationToken ct) =>
        Map(await service.SaveBatch(itemId, null, command, ct));

    [HttpPut("api/catalog-items/{itemId:guid}/batches/{id:guid}"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<BatchDto>> UpdateBatch(Guid itemId, Guid id, SaveBatchCommand command, CancellationToken ct) =>
        Map(await service.SaveBatch(itemId, id, command, ct));
}
