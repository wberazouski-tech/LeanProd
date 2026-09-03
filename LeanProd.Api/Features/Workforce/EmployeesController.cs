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

[ApiController, Route("api/employees"), Authorize(Policy = Permissions.WorkforceView)]
public sealed class EmployeesController(IEmployeeService service) : WorkforceControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MasterDataPage<EmployeeSummary>>> List(int page = 1, int pageSize = 20,
        string? search = null, bool? isActive = null, Guid? departmentId = null,
        CancellationToken ct = default) =>
        Ok(await service.GetEmployeesAsync(
            new(Math.Max(1, page), Math.Clamp(pageSize, 1, 5000), search, isActive, departmentId), ct));

    [HttpGet("options")]
    public async Task<ActionResult<IReadOnlyCollection<EmployeeOption>>> Options(CancellationToken ct) =>
        Ok(await service.GetEmployeeOptionsAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeDetails>> Get(Guid id, CancellationToken ct)
    {
        var item = await service.GetEmployeeAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("{id:guid}/brigade-history")]
    public async Task<ActionResult<IReadOnlyCollection<BrigadeMembershipDetails>>> History(
        Guid id, CancellationToken ct) => Ok(await service.GetBrigadeHistoryAsync(id, ct));

    [HttpPost, Authorize(Policy = Permissions.WorkforceManage)]
    public async Task<ActionResult<EmployeeDetails>> Create(SaveEmployeeRequest request, CancellationToken ct) =>
        Map(await service.CreateEmployeeAsync(Command(request), ct));

    [HttpPut("{id:guid}"), Authorize(Policy = Permissions.WorkforceManage)]
    public async Task<ActionResult<EmployeeDetails>> Update(
        Guid id, SaveEmployeeRequest request, CancellationToken ct) =>
        Map(await service.UpdateEmployeeAsync(id, Command(request), ct));

    [HttpPost("{id:guid}/activate"), Authorize(Policy = Permissions.WorkforceManage)]
    public async Task<ActionResult<bool>> Activate(Guid id, CancellationToken ct) =>
        Map(await service.SetEmployeeActiveAsync(id, true, ct));

    [HttpPost("{id:guid}/deactivate"), Authorize(Policy = Permissions.WorkforceManage)]
    public async Task<ActionResult<bool>> Deactivate(Guid id, CancellationToken ct) =>
        Map(await service.SetEmployeeActiveAsync(id, false, ct));

    private static SaveEmployeeCommand Command(SaveEmployeeRequest x) => new(x.PersonnelNumber,
        x.LastName, x.FirstName, x.MiddleName, x.Position, x.DepartmentId, x.RowVersion);
}
