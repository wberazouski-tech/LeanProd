using System;
using System.Security.Claims;

namespace API.Exctensions;

public static class ClaimsPrincipleExtensions
{
    public static string GetUsername(this ClaimsPrincipal user)
    {
        var username = user.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? throw new Exception("User does not have a username claim.");
        return username;

    }

}
