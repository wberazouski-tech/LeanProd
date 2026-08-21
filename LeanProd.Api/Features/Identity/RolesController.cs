using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeanProd.Api.Common.Controllers;
using LeanProd.Application.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeanProd.Api.Features.Identity;

[Authorize(Policy = Permissions.UsersManage)]
public sealed class RolesController(IUserAdministrationService users) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<RoleDetails>>> GetRoles(CancellationToken cancellationToken) =>
        Ok(await users.GetRolesAsync(cancellationToken));
}
