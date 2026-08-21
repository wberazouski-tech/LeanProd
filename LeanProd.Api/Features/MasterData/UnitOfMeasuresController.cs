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

[ApiController, Route("api/unit-of-measures"), Authorize(Policy = Permissions.MasterDataView)]
public sealed class UnitOfMeasuresController(IUnitOfMeasureService service) : MasterDataControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MasterDataPage<UnitOfMeasureSummary>>> List(
        int page = 1, int pageSize = 20, string? search = null, bool? isActive = null,
        string language = "en", CancellationToken ct = default)
    {
        if (page < 1 || pageSize is < 1 or > 100) return Problem(statusCode: 400, title: "Invalid paging");
        return Ok(await service.GetUnitsAsync(new(page, pageSize, search, isActive), language, ct));
    }

    [HttpGet("catalog")]
    public async Task<ActionResult<IReadOnlyCollection<UnitCatalogOption>>> Catalog(string? search = null, CancellationToken ct = default) =>
        Ok(await service.GetCatalogAsync(search, ct));

    [HttpGet("options")]
    public async Task<ActionResult<IReadOnlyCollection<UnitOfMeasureSummary>>> Options(string language = "en", CancellationToken ct = default) =>
        Ok(await service.GetUnitOptionsAsync(language, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UnitOfMeasureDetails>> Get(Guid id, string language = "en", CancellationToken ct = default)
    {
        var value = await service.GetUnitAsync(id, language, ct);
        return value is null ? NotFound() : Ok(value);
    }

    [HttpPost, Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<UnitOfMeasureDetails>> Create(CreateUnitOfMeasureRequest request, CancellationToken ct)
    {
        var result = await service.CreateUnitAsync(new(request.CatalogCode, request.QuantityType,
            request.DecimalPlaces, request.LanguageCode, request.LocalizedName), ct);
        return result.Succeeded ? CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value) : Map(result);
    }

    [HttpPut("{id:guid}"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<UnitOfMeasureDetails>> Update(Guid id, UpdateUnitOfMeasureRequest request, CancellationToken ct) =>
        Map(await service.UpdateUnitAsync(id, new(request.QuantityType, request.DecimalPlaces,
            request.LanguageCode, request.LocalizedName, request.RowVersion), ct));

    [HttpPost("{id:guid}/activate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Activate(Guid id, CancellationToken ct) => Map(await service.SetUnitActiveAsync(id, true, ct));

    [HttpPost("{id:guid}/deactivate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Deactivate(Guid id, CancellationToken ct) => Map(await service.SetUnitActiveAsync(id, false, ct));

    [HttpGet("conversions")]
    public async Task<ActionResult<IReadOnlyCollection<UnitConversionDetails>>> Conversions(CancellationToken ct) =>
        Ok(await service.GetConversionsAsync(ct));

    [HttpPost("conversions"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<UnitConversionDetails>> CreateConversion(CreateUnitConversionRequest request, CancellationToken ct) =>
        Map(await service.CreateConversionAsync(new(request.FromUnitId, request.ToUnitId, request.Multiplier, request.Offset), ct));

    [HttpDelete("conversions/{id:guid}"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> DeleteConversion(Guid id, CancellationToken ct) =>
        Map(await service.DeleteConversionAsync(id, ct));

    [HttpPost("conversions/calculate")]
    public async Task<ActionResult<UnitConversionCalculation>> Calculate(ConvertUnitRequest request, CancellationToken ct) =>
        Map(await service.ConvertAsync(new(request.FromUnitId, request.ToUnitId, request.Value), ct));
}
