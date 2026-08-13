using System;
using System.Security.Claims;
using LeanProd.Application.Common.Abstractions;
using Microsoft.AspNetCore.Http;

namespace LeanProd.Api.Common.Identity;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }
}
