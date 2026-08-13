using LeanProd.Application.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace LeanProd.Api.Common.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var roles = context.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value);
        if (RolePermissionMatrix.HasPermission(roles, requirement.Permission))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
