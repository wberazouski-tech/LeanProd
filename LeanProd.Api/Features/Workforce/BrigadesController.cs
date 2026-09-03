using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Api.Features.Workforce.Contracts;
using LeanProd.Application.Features.Identity;
using LeanProd.Application.Features.MasterData;
using LeanProd.Application.Features.Workforce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Features.Workforce;

[ApiController, Route("api/brigades"), Authorize(Policy = Permissions.WorkforceView)]
public sealed class BrigadesController(IBrigadeService service) : WorkforceControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MasterDataPage<BrigadeSummary>>> List(int page = 1, int pageSize = 20,
        string? search = null, bool? isActive = null, Guid? departmentId = null,
        CancellationToken ct = default) =>
        Ok(await service.GetBrigadesAsync(
            new(Math.Max(1, page), Math.Clamp(pageSize, 1, 5000), search, isActive, departmentId), ct));

    [HttpGet("options")]
    public async Task<ActionResult<IReadOnlyCollection<BrigadeOption>>> Options(CancellationToken ct) =>
        Ok(await service.GetBrigadeOptionsAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BrigadeDetails>> Get(Guid id, CancellationToken ct)
    {
        var item = await service.GetBrigadeAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("{id:guid}/memberships")]
    public async Task<ActionResult<IReadOnlyCollection<BrigadeMembershipDetails>>> Memberships(
        Guid id, bool history = false, CancellationToken ct = default) =>
        Ok(await service.GetMembershipsAsync(id, history, ct));

    [HttpPost, Authorize(Policy = Permissions.WorkforceManage)]
    public async Task<ActionResult<BrigadeDetails>> Create(SaveBrigadeRequest request, CancellationToken ct) =>
        Map(await service.CreateBrigadeAsync(Command(request), ct));

    [HttpPut("{id:guid}"), Authorize(Policy = Permissions.WorkforceManage)]
    public async Task<ActionResult<BrigadeDetails>> Update(
        Guid id, SaveBrigadeRequest request, CancellationToken ct) =>
        Map(await service.UpdateBrigadeAsync(id, Command(request), ct));

    [HttpPost("{id:guid}/activate"), Authorize(Policy = Permissions.WorkforceManage)]
    public async Task<ActionResult<bool>> Activate(Guid id, CancellationToken ct) =>
        Map(await service.SetBrigadeActiveAsync(id, true, ct));

    [HttpPost("{id:guid}/deactivate"), Authorize(Policy = Permissions.WorkforceManage)]
    public async Task<ActionResult<bool>> Deactivate(Guid id, CancellationToken ct) =>
        Map(await service.SetBrigadeActiveAsync(id, false, ct));

    [HttpPost("{id:guid}/memberships"), Authorize(Policy = Permissions.WorkforceManage)]
    public async Task<ActionResult<BrigadeMembershipDetails>> AddMembership(Guid id,
        AddBrigadeMembershipRequest request, CancellationToken ct) =>
        Map(await service.AddMembershipAsync(id, new(request.EmployeeId, request.StartedAtUtc,
            request.EndedAtUtc, request.LaborParticipationCoefficient), ct));

    [HttpPost("{id:guid}/memberships/{membershipId:guid}/close"),
     Authorize(Policy = Permissions.WorkforceManage)]
    public async Task<ActionResult<BrigadeMembershipDetails>> CloseMembership(Guid id, Guid membershipId,
        CloseBrigadeMembershipRequest request, CancellationToken ct) =>
        Map(await service.CloseMembershipAsync(id, membershipId,
            new(request.EndedAtUtc, request.RowVersion), ct));

    [HttpPost("{id:guid}/memberships/{membershipId:guid}/change-coefficient"),
     Authorize(Policy = Permissions.WorkforceManage)]
    public async Task<ActionResult<BrigadeMembershipDetails>> ChangeCoefficient(Guid id, Guid membershipId,
        ChangeMembershipCoefficientRequest request, CancellationToken ct) =>
        Map(await service.ChangeCoefficientAsync(id, membershipId,
            new(request.EffectiveFromUtc, request.LaborParticipationCoefficient, request.RowVersion), ct));

    private static SaveBrigadeCommand Command(SaveBrigadeRequest x) =>
        new(x.Code, x.Name, x.Description, x.DepartmentId, x.RowVersion);
}
