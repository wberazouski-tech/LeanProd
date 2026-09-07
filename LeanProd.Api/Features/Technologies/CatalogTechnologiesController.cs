using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Api.Features.MasterData;
using LeanProd.Api.Features.Technologies.Contracts;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.MasterData;
using LeanProd.Application.Features.Technologies;
using LeanProd.Domain.Technologies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Features.Technologies;

[ApiController, Route("api/catalog-technologies"), Authorize(Policy = Permissions.MasterDataView)]
public sealed class CatalogTechnologiesController(ICatalogTechnologyService service) : MasterDataControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MasterDataPage<CatalogTechnologySummary>>> List(
        int page = 1, int pageSize = 100, string? search = null, bool? isActive = null,
        Guid? catalogItemId = null, Guid? catalogItemClassId = null, CancellationToken ct = default)
    {
        if (page < 1 || pageSize is < 1 or > 5000) return Problem(statusCode: 400, title: "Invalid paging");
        return Ok(await service.GetTechnologiesAsync(
            new(page, pageSize, search, isActive, catalogItemId, catalogItemClassId), ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CatalogTechnologyDetails>> Get(Guid id, CancellationToken ct)
    {
        var item = await service.GetTechnologyAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost, Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<CatalogTechnologyDetails>> Create(
        SaveCatalogTechnologyRequest request, CancellationToken ct)
    {
        var result = await service.CreateTechnologyAsync(Command(request), ct);
        return result.Succeeded
            ? CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value)
            : Map(result);
    }

    [HttpPut("{id:guid}"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<CatalogTechnologyDetails>> Update(
        Guid id, SaveCatalogTechnologyRequest request, CancellationToken ct) =>
        Map(await service.UpdateTechnologyAsync(id, Command(request), ct));

    [HttpPost("{id:guid}/activate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Activate(Guid id, CancellationToken ct) =>
        Map(await service.SetTechnologyActiveAsync(id, true, ct));

    [HttpPost("{id:guid}/deactivate"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<bool>> Deactivate(Guid id, CancellationToken ct) =>
        Map(await service.SetTechnologyActiveAsync(id, false, ct));

    [HttpGet("stage-templates")]
    public async Task<ActionResult<IReadOnlyCollection<TechnologyStageTemplateDetails>>> StageTemplates(
        bool activeOnly = false, CancellationToken ct = default) =>
        Ok(await service.GetStageTemplatesAsync(activeOnly, ct));

    [HttpGet("technology-stages")]
    public async Task<ActionResult<IReadOnlyCollection<TechnologyStageDetails>>> TechnologyStages(
        bool activeOnly = true, CancellationToken ct = default) =>
        Ok(await service.GetTechnologyStagesAsync(activeOnly, ct));

    [HttpGet("stage-duplicates")]
    public async Task<ActionResult<bool>> HasStageDuplicate(
        string name, Guid departmentId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return Problem(statusCode: 400, title: "Stage name is required.");
        return Ok(await service.HasTechnologyStageDuplicateAsync(name, departmentId, ct));
    }

    [HttpPost("stage-templates"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<TechnologyStageTemplateDetails>> CreateStageTemplate(
        SaveTechnologyStageTemplateRequest request, CancellationToken ct) =>
        Map(await service.SaveStageTemplateAsync(null, TemplateCommand(request), ct));

    [HttpPut("stage-templates/{id:guid}"), Authorize(Policy = Permissions.MasterDataManage)]
    public async Task<ActionResult<TechnologyStageTemplateDetails>> UpdateStageTemplate(
        Guid id, SaveTechnologyStageTemplateRequest request, CancellationToken ct) =>
        Map(await service.SaveStageTemplateAsync(id, TemplateCommand(request), ct));

    private static SaveCatalogTechnologyCommand Command(SaveCatalogTechnologyRequest request) => new(
        request.Code, request.Name, request.CatalogItemId, request.CatalogItemClassId,
        request.VersionNo, request.ValidFrom, request.ValidTo, request.IsDefault,
        request.Status ?? CatalogTechnologyStatus.InDevelopment,
        request.Description, request.Stages.Select(StageCommand).ToArray(),
        request.StageTransitions.Select(TransitionCommand).ToArray(), request.RowVersion);

    private static SaveCatalogTechnologyStageCommand StageCommand(
        SaveCatalogTechnologyStageRequest request) => new(
        request.Id, request.TechnologyStageId, request.TechnologyStageCode, request.TechnologyStageName, request.StageNumber,
        request.PlannedDurationMinutes, request.TechnologyStageDepartmentId, request.EquipmentId,
        request.Description, request.Materials.Select(MaterialCommand).ToArray(),
        request.Outputs.Select(OutputCommand).ToArray(), request.Operations.Select(OperationCommand).ToArray());

    private static SaveCatalogTechnologyStageTransitionCommand TransitionCommand(
        SaveCatalogTechnologyStageTransitionRequest request) => new(
        request.Id, request.FromCatalogTechnologyStageId, request.ToCatalogTechnologyStageId);

    private static SaveCatalogTechnologyMaterialCommand MaterialCommand(
        SaveCatalogTechnologyMaterialRequest request) => new(
        request.Id, request.CatalogItemId, request.UnitOfMeasureId, request.Quantity,
        request.ConsumptionTrackingMode, request.DefaultSourceStorageLocationId,
        request.ScrapPercent, request.IsOptional, request.Note,
        request.RouteSteps.Select(RouteStepCommand).ToArray());

    private static SaveCatalogTechnologyStageOutputCommand OutputCommand(
        SaveCatalogTechnologyStageOutputRequest request) => new(
        request.Id, request.CatalogItemId, request.UnitOfMeasureId, request.Quantity,
        request.ReceiptStorageLocationId, request.IsPrimary, request.Note);

    private static SaveCatalogTechnologyOperationCommand OperationCommand(
        SaveCatalogTechnologyOperationRequest request) => new(
        request.Id, request.Code, request.Name, request.DepartmentId, request.EquipmentId,
        request.SetupMinutes, request.RunMinutes, request.LaborMinutes, request.Workers, request.Note);

    private static SaveCatalogTechnologyMaterialSupplyRouteStepCommand RouteStepCommand(
        SaveCatalogTechnologyMaterialSupplyRouteStepRequest request) => new(
        request.Id, request.LineNo, request.FromStorageLocationId, request.ToStorageLocationId,
        request.IsConsumptionPoint, request.MovementKind, request.LeadTimeMinutes, request.Note);

    private static SaveTechnologyStageTemplateCommand TemplateCommand(
        SaveTechnologyStageTemplateRequest request) => new(
        request.Code, request.Name, request.Description, request.IsActive, request.RowVersion);
}
