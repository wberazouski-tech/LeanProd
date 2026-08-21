using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LeanProd.Api.Common.Controllers;
using LeanProd.Api.Features.Identity.Contracts;
using LeanProd.Application.Features.Identity;
using LeanProd.Infrastructure.Common.Persistence;
using LeanProd.Infrastructure.Features.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LeanProd.Api.Features.Identity;

public sealed class IdentityController(
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager,
    LeanProdDbContext dbContext,
    TokenService tokenService,
    IConfiguration configuration) : BaseApiController
{
    private const string RefreshCookieName = "__Host-LeanProd.Refresh";

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<CurrentUserResponse>> Register(RegisterRequest request)
    {
        if (!configuration.GetValue("Identity:AllowPublicRegistration", false))
            return Problem(statusCode: 403, title: "Registration disabled",
                detail: "Public registration is disabled. Ask an administrator to create the account.");
        var preferredLanguage = request.PreferredLanguage ?? SupportedLanguages.Belarusian;
        if (!SupportedLanguages.IsSupported(preferredLanguage))
            return Problem(statusCode: 400, title: "Invalid language",
                detail: "Preferred language must be 'be' or 'en'.");

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var email = request.Email.Trim().ToLowerInvariant();
        if (await userManager.FindByEmailAsync(email) is not null)
            return Conflict(new { message = "A user with this email already exists." });

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            PreferredLanguage = preferredLanguage.ToLowerInvariant(),
            IsActive = true
        };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new
            {
                message = "Registration failed.",
                errors = result.Errors.Select(error => error.Description).ToArray()
            });

        var role = await userManager.Users.CountAsync() == 1
            ? RoleNames.SystemAdministrator : RoleNames.Viewer;
        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return Problem("Could not assign the default user role.");
        }

        var response = await IssueTokenPair(user);
        await transaction.CommitAsync();
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<CurrentUserResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive) return Unauthorized();
        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded) return Unauthorized();
        user.LastLoginAtUtc = DateTime.UtcNow;
        return Ok(await IssueTokenPair(user));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<CurrentUserResponse>> Refresh()
    {
        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var rawToken)) return Unauthorized();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var hash = TokenService.Hash(rawToken);
        var stored = await dbContext.RefreshTokens.Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == hash);
        if (stored is null) return Unauthorized();

        if (!stored.IsActive)
        {
            if (stored.RevokedAtUtc is not null)
            {
                var family = await dbContext.RefreshTokens
                    .Where(x => x.UserId == stored.UserId && x.FamilyId == stored.FamilyId && x.RevokedAtUtc == null)
                    .ToListAsync();
                foreach (var token in family) token.RevokedAtUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            DeleteRefreshCookie();
            return Unauthorized();
        }

        if (!stored.User.IsActive) return Unauthorized();
        stored.RevokedAtUtc = DateTime.UtcNow;
        var pair = await tokenService.CreateAsync(stored.User, stored.FamilyId);
        stored.ReplacedByTokenHash = TokenService.Hash(pair.RefreshToken);
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        WriteRefreshCookie(pair.RefreshToken, pair.RefreshTokenExpiresAtUtc);
        return Ok(await ToResponse(stored.User, pair));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue(RefreshCookieName, out var rawToken))
        {
            var hash = TokenService.Hash(rawToken);
            var stored = await dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == hash);
            if (stored is not null && stored.RevokedAtUtc is null)
            {
                stored.RevokedAtUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }
        }
        DeleteRefreshCookie();
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<object>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var id)) return Unauthorized();
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null || !user.IsActive) return Unauthorized();
        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        return Ok(new
        {
            user.Id,
            user.Email,
            user.DisplayName,
            user.PreferredLanguage,
            user.DefaultDepartmentId,
            user.DefaultStorageLocationId,
            roles,
            permissions = RolePermissionMatrix.GetPermissions(roles)
        });
    }

    [Authorize]
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(UpdatePreferencesRequest request)
    {
        if (!SupportedLanguages.IsSupported(request.PreferredLanguage))
            return Problem(statusCode: 400, title: "Invalid language",
                detail: "Preferred language must be 'be' or 'en'.");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userManager.FindByIdAsync(userId!);
        if (user is null || !user.IsActive) return Unauthorized();
        user.PreferredLanguage = request.PreferredLanguage.ToLowerInvariant();
        user.UpdatedAtUtc = DateTime.UtcNow;
        user.UpdatedByUserId = user.Id;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded ? NoContent() : Problem("Could not save interface language.");
    }

    private async Task<CurrentUserResponse> IssueTokenPair(AppUser user)
    {
        var pair = await tokenService.CreateAsync(user);
        await dbContext.SaveChangesAsync();
        WriteRefreshCookie(pair.RefreshToken, pair.RefreshTokenExpiresAtUtc);
        return await ToResponse(user, pair);
    }

    private async Task<CurrentUserResponse> ToResponse(AppUser user, TokenPair pair)
    {
        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        return new(user.Id, user.Email!, user.DisplayName, user.PreferredLanguage,
            user.DefaultDepartmentId, user.DefaultStorageLocationId, roles,
            [.. RolePermissionMatrix.GetPermissions(roles)], pair.AccessToken, pair.AccessTokenExpiresAtUtc);
    }

    private void WriteRefreshCookie(string token, DateTime expires) => Response.Cookies.Append(
        RefreshCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expires,
            Path = "/"
        });

    private void DeleteRefreshCookie() => Response.Cookies.Delete(RefreshCookieName,
        new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Path = "/" });
}
