using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Api.Common.Controllers;
using LeanProd.Api.Features.Identity.Contracts;
using LeanProd.Application.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Features.Identity;

[Authorize(Policy = Permissions.UsersManage)]
public sealed class UsersController(IUserAdministrationService users) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserSummary>>> GetUsers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] bool? isActive = null,
        [FromQuery] string? role = null, CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 5000)
            return Problem(statusCode: 400, title: "Invalid paging", detail: "Page must be positive and pageSize must be between 1 and 5000.");

        return Ok(await users.GetUsersAsync(
            new UserListQuery(page, pageSize, search, isActive, role), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDetails>> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDetails>> Create(
        CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await users.CreateUserAsync(
            new CreateUserCommand(request.Email, request.DisplayName, request.TemporaryPassword,
                request.PreferredLanguage ?? SupportedLanguages.Belarusian,
                request.DefaultDepartmentId, request.DefaultStorageLocationId, request.Roles),
            ActorId(), HttpContext.TraceIdentifier, cancellationToken);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetUser), new { id = result.Value!.Id }, result.Value)
            : Map(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDetails>> Update(
        Guid id, UpdateUserRequest request, CancellationToken cancellationToken) =>
        Map(await users.UpdateUserAsync(id,
            new UpdateUserCommand(request.Email, request.DisplayName, request.PreferredLanguage,
                request.DefaultDepartmentId, request.DefaultStorageLocationId, request.ConcurrencyStamp),
            ActorId(), HttpContext.TraceIdentifier, cancellationToken));

    [HttpPut("{id:guid}/roles")]
    public async Task<ActionResult<UserDetails>> SetRoles(
        Guid id, SetUserRolesRequest request, CancellationToken cancellationToken) =>
        Map(await users.SetRolesAsync(id,
            new SetUserRolesCommand(request.Roles, request.ConcurrencyStamp),
            ActorId(), HttpContext.TraceIdentifier, cancellationToken));

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<bool>> Activate(Guid id, CancellationToken cancellationToken) =>
        Map(await users.SetActiveAsync(id, true, ActorId(), HttpContext.TraceIdentifier, cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<bool>> Deactivate(Guid id, CancellationToken cancellationToken) =>
        Map(await users.SetActiveAsync(id, false, ActorId(), HttpContext.TraceIdentifier, cancellationToken));

    [HttpPost("{id:guid}/reset-password")]
    public async Task<ActionResult<bool>> ResetPassword(
        Guid id, ResetUserPasswordRequest request, CancellationToken cancellationToken) =>
        Map(await users.ResetPasswordAsync(id, request.TemporaryPassword, ActorId(),
            HttpContext.TraceIdentifier, cancellationToken));

    private Guid ActorId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private ActionResult<T> Map<T>(UserAdministrationResult<T> result)
    {
        if (result.Succeeded) return Ok(result.Value);
        var status = result.Error switch
        {
            UserAdministrationError.NotFound => 404,
            UserAdministrationError.Validation => 400,
            _ => 409
        };
        return Problem(statusCode: status, title: result.Error.ToString(),
            detail: result.ValidationErrors is null
                ? result.Message
                : $"{result.Message} {string.Join(" ", result.ValidationErrors)}");
    }
}
