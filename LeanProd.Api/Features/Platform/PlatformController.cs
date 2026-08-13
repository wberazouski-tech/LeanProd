using LeanProd.Api.Common.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LeanProd.Application.Features.Identity;

namespace LeanProd.Api.Features.Platform;

public sealed class PlatformController : BaseApiController
{
    [HttpGet("info")]
    public ActionResult<object> GetPlatformInfo() => Ok(new
    {
        name = "LeanProd",
        status = "foundation",
        apiVersion = "1"
    });

    [Authorize(Policy = Permissions.ReportsView)]
    [HttpGet("workspace-access")]
    public IActionResult GetWorkspaceAccess() => NoContent();

    [Authorize(Policy = Permissions.UsersManage)]
    [HttpGet("administration-access")]
    public IActionResult GetAdministrationAccess() => NoContent();
}
