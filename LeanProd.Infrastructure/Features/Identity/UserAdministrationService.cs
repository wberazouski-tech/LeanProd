using System.Data;
using System.Text.Json;
using LeanProd.Application.Features.Identity;
using LeanProd.Infrastructure.Common.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LeanProd.Infrastructure.Features.Identity;

public sealed class UserAdministrationService(
    LeanProdDbContext dbContext,
    UserManager<AppUser> userManager,
    TimeProvider timeProvider) : IUserAdministrationService
{
    public async Task<PagedResult<UserSummary>> GetUsersAsync(
        UserListQuery query, CancellationToken cancellationToken)
    {
        var users = userManager.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            users = users.Where(x => x.Email!.Contains(search) || x.DisplayName.Contains(search));
        }
        if (query.IsActive is not null) users = users.Where(x => x.IsActive == query.IsActive);
        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var normalizedRole = query.Role.Trim().ToUpperInvariant();
            users = users.Where(user => dbContext.UserRoles.Any(userRole =>
                userRole.UserId == user.Id && dbContext.Roles.Any(role =>
                    role.Id == userRole.RoleId && role.NormalizedName == normalizedRole)));
        }

        var total = await users.CountAsync(cancellationToken);
        var pageUsers = await users.OrderBy(x => x.DisplayName).ThenBy(x => x.Email)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(cancellationToken);
        var roleMap = await GetRoleMap(pageUsers.Select(x => x.Id), cancellationToken);
        var items = pageUsers.Select(user => new UserSummary(
            user.Id, user.Email!, user.DisplayName, user.IsActive,
            user.PreferredLanguage, user.DefaultDepartmentId, user.DefaultStorageLocationId,
            roleMap.GetValueOrDefault(user.Id, []), user.CreatedAtUtc, user.LastLoginAtUtc)).ToArray();
        return new(items, query.Page, query.PageSize, total);
    }

    public async Task<UserDetails?> GetUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManager.Users.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return user is null ? null : await ToDetails(user, cancellationToken);
    }

    public Task<IReadOnlyCollection<RoleDetails>> GetRolesAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<RoleDetails>>(RoleNames.All
            .Order(StringComparer.Ordinal)
            .Select(name => new RoleDetails(name, RolePermissionMatrix.GetPermissions([name])))
            .ToArray());

    public async Task<UserAdministrationResult<UserDetails>> CreateUserAsync(
        CreateUserCommand command, Guid actorUserId, string traceId, CancellationToken cancellationToken)
    {
        var roleError = ValidateRoles(command.Roles);
        if (roleError is not null) return UserAdministrationResult<UserDetails>.Failure(
            UserAdministrationError.Validation, roleError);
        if (!SupportedLanguages.IsSupported(command.PreferredLanguage))
            return InvalidLanguage<UserDetails>();
        var defaultsError = await ValidateDefaults(command.DefaultDepartmentId, command.DefaultStorageLocationId, cancellationToken);
        if (defaultsError is not null) return UserAdministrationResult<UserDetails>.Failure(UserAdministrationError.Validation, defaultsError);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var email = NormalizeEmail(command.Email);
        if (await userManager.FindByEmailAsync(email) is not null)
            return UserAdministrationResult<UserDetails>.Failure(
                UserAdministrationError.Conflict, "A user with this email already exists.");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = command.DisplayName.Trim(),
            PreferredLanguage = command.PreferredLanguage.ToLowerInvariant(),
            DefaultDepartmentId = command.DefaultDepartmentId,
            DefaultStorageLocationId = command.DefaultStorageLocationId,
            IsActive = true,
            CreatedAtUtc = now,
            CreatedByUserId = actorUserId
        };
        var created = await userManager.CreateAsync(user, command.TemporaryPassword);
        if (!created.Succeeded) return IdentityFailure<UserDetails>(created, "Could not create the user.");

        var roles = NormalizeRoles(command.Roles);
        var assigned = await userManager.AddToRolesAsync(user, roles);
        if (!assigned.Succeeded) return IdentityFailure<UserDetails>(assigned, "Could not assign roles.");

        AddAudit(actorUserId, user.Id, "UserCreated", traceId, new { user.Email, Roles = roles });
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return UserAdministrationResult<UserDetails>.Success(await ToDetails(user, cancellationToken));
    }

    public async Task<UserAdministrationResult<UserDetails>> UpdateUserAsync(
        Guid id, UpdateUserCommand command, Guid actorUserId, string traceId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound<UserDetails>();
        if (!SupportedLanguages.IsSupported(command.PreferredLanguage))
            return InvalidLanguage<UserDetails>();
        var defaultsError = await ValidateDefaults(command.DefaultDepartmentId, command.DefaultStorageLocationId, cancellationToken);
        if (defaultsError is not null) return UserAdministrationResult<UserDetails>.Failure(UserAdministrationError.Validation, defaultsError);
        if (!string.Equals(user.ConcurrencyStamp, command.ConcurrencyStamp, StringComparison.Ordinal))
            return ConcurrencyConflict<UserDetails>();

        var email = NormalizeEmail(command.Email);
        var duplicate = await userManager.FindByEmailAsync(email);
        if (duplicate is not null && duplicate.Id != id)
            return UserAdministrationResult<UserDetails>.Failure(
                UserAdministrationError.Conflict, "A user with this email already exists.");

        var previous = new { user.Email, user.DisplayName, user.PreferredLanguage, user.DefaultDepartmentId, user.DefaultStorageLocationId };
        user.Email = email;
        user.UserName = email;
        user.DisplayName = command.DisplayName.Trim();
        user.PreferredLanguage = command.PreferredLanguage.ToLowerInvariant();
        user.DefaultDepartmentId = command.DefaultDepartmentId;
        user.DefaultStorageLocationId = command.DefaultStorageLocationId;
        user.UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        user.UpdatedByUserId = actorUserId;
        var updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded) return IdentityFailure<UserDetails>(updated, "Could not update the user.");

        AddAudit(actorUserId, user.Id, "UserProfileUpdated", traceId,
            new { Previous = previous, Current = new { user.Email, user.DisplayName, user.PreferredLanguage, user.DefaultDepartmentId, user.DefaultStorageLocationId } });
        await dbContext.SaveChangesAsync(cancellationToken);
        return UserAdministrationResult<UserDetails>.Success(await ToDetails(user, cancellationToken));
    }

    public async Task<UserAdministrationResult<UserDetails>> SetRolesAsync(
        Guid id, SetUserRolesCommand command, Guid actorUserId, string traceId,
        CancellationToken cancellationToken)
    {
        var roleError = ValidateRoles(command.Roles);
        if (roleError is not null) return UserAdministrationResult<UserDetails>.Failure(
            UserAdministrationError.Validation, roleError);
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound<UserDetails>();
        if (!string.Equals(user.ConcurrencyStamp, command.ConcurrencyStamp, StringComparison.Ordinal))
            return ConcurrencyConflict<UserDetails>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var existing = (await userManager.GetRolesAsync(user)).ToArray();
        var desired = NormalizeRoles(command.Roles);
        if (existing.Contains(RoleNames.SystemAdministrator) &&
            !desired.Contains(RoleNames.SystemAdministrator) &&
            !await HasAnotherActiveAdministrator(id, cancellationToken))
            return LastAdministrator<UserDetails>();

        var removed = await userManager.RemoveFromRolesAsync(user, existing.Except(desired));
        if (!removed.Succeeded) return IdentityFailure<UserDetails>(removed, "Could not remove roles.");
        var added = await userManager.AddToRolesAsync(user, desired.Except(existing));
        if (!added.Succeeded) return IdentityFailure<UserDetails>(added, "Could not assign roles.");

        user.UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        user.UpdatedByUserId = actorUserId;
        var updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded) return IdentityFailure<UserDetails>(updated, "Could not update the user version.");
        AddAudit(actorUserId, id, "UserRolesChanged", traceId, new { Previous = existing, Current = desired });
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return UserAdministrationResult<UserDetails>.Success(await ToDetails(user, cancellationToken));
    }

    public async Task<UserAdministrationResult<bool>> SetActiveAsync(
        Guid id, bool isActive, Guid actorUserId, string traceId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound<bool>();
        if (!isActive && id == actorUserId)
            return UserAdministrationResult<bool>.Failure(
                UserAdministrationError.SelfDeactivation, "You cannot deactivate your own account.");

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        if (!isActive && await userManager.IsInRoleAsync(user, RoleNames.SystemAdministrator) &&
            !await HasAnotherActiveAdministrator(id, cancellationToken))
            return LastAdministrator<bool>();

        user.IsActive = isActive;
        user.UpdatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        user.UpdatedByUserId = actorUserId;
        var updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded) return IdentityFailure<bool>(updated, "Could not change user status.");

        if (!isActive)
        {
            var activeTokens = await dbContext.RefreshTokens
                .Where(x => x.UserId == id && x.RevokedAtUtc == null).ToListAsync(cancellationToken);
            foreach (var token in activeTokens) token.RevokedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        }
        AddAudit(actorUserId, id, isActive ? "UserActivated" : "UserDeactivated", traceId, null);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return UserAdministrationResult<bool>.Success(true);
    }

    public async Task<UserAdministrationResult<bool>> ResetPasswordAsync(
        Guid id, string temporaryPassword, Guid actorUserId, string traceId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound<bool>();
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, temporaryPassword);
        if (!result.Succeeded) return IdentityFailure<bool>(result, "Could not reset the password.");

        var activeTokens = await dbContext.RefreshTokens
            .Where(x => x.UserId == id && x.RevokedAtUtc == null).ToListAsync(cancellationToken);
        foreach (var token in activeTokens) token.RevokedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        AddAudit(actorUserId, id, "UserPasswordReset", traceId, null);
        await dbContext.SaveChangesAsync(cancellationToken);
        return UserAdministrationResult<bool>.Success(true);
    }

    private async Task<UserDetails> ToDetails(AppUser user, CancellationToken cancellationToken)
    {
        var roles = (await GetRoleMap([user.Id], cancellationToken)).GetValueOrDefault(user.Id, []);
        return new(user.Id, user.Email!, user.DisplayName, user.IsActive, user.PreferredLanguage,
            user.DefaultDepartmentId, user.DefaultStorageLocationId, roles,
            RolePermissionMatrix.GetPermissions(roles), user.CreatedAtUtc, user.UpdatedAtUtc,
            user.LastLoginAtUtc, user.ConcurrencyStamp!);
    }

    private async Task<Dictionary<Guid, string[]>> GetRoleMap(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.ToArray();
        return (await (from userRole in dbContext.UserRoles
                join role in dbContext.Roles on userRole.RoleId equals role.Id
                where ids.Contains(userRole.UserId)
                select new { userRole.UserId, Role = role.Name! })
            .ToListAsync(cancellationToken))
            .GroupBy(x => x.UserId)
            .ToDictionary(x => x.Key, x => x.Select(item => item.Role).Order().ToArray());
    }

    private async Task<bool> HasAnotherActiveAdministrator(Guid excludedId, CancellationToken cancellationToken) =>
        await (from user in dbContext.Users
            join userRole in dbContext.UserRoles on user.Id equals userRole.UserId
            join role in dbContext.Roles on userRole.RoleId equals role.Id
            where user.Id != excludedId && user.IsActive && role.Name == RoleNames.SystemAdministrator
            select user.Id).AnyAsync(cancellationToken);

    private async Task<string?> ValidateDefaults(Guid? departmentId, Guid? storageLocationId, CancellationToken cancellationToken)
    {
        if (departmentId is not null && !await dbContext.Departments.AnyAsync(
                x => x.Id == departmentId && x.IsActive, cancellationToken))
            return "The default department must be active.";
        if (storageLocationId is not null && !await dbContext.StorageLocations.AnyAsync(
                x => x.Id == storageLocationId && x.IsActive, cancellationToken))
            return "The default storage location must be active.";
        return null;
    }

    private void AddAudit(Guid actor, Guid target, string action, string traceId, object? details) =>
        dbContext.SecurityAuditEvents.Add(new SecurityAuditEvent
        {
            ActorUserId = actor,
            TargetUserId = target,
            Action = action,
            TraceId = traceId,
            Details = details is null ? null : JsonSerializer.Serialize(details),
            OccurredAtUtc = timeProvider.GetUtcNow().UtcDateTime
        });

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    private static string[] NormalizeRoles(IEnumerable<string> roles) => roles
        .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    private static string? ValidateRoles(IReadOnlyCollection<string> roles) => roles.Count == 0
        ? "At least one role is required."
        : roles.Any(role => !RoleNames.All.Contains(role, StringComparer.Ordinal))
            ? "One or more roles are unknown."
            : null;
    private static UserAdministrationResult<T> IdentityFailure<T>(IdentityResult result, string message) =>
        UserAdministrationResult<T>.Failure(UserAdministrationError.Validation, message,
            result.Errors.Select(x => x.Description).ToArray());
    private static UserAdministrationResult<T> NotFound<T>() =>
        UserAdministrationResult<T>.Failure(UserAdministrationError.NotFound, "User was not found.");
    private static UserAdministrationResult<T> ConcurrencyConflict<T>() =>
        UserAdministrationResult<T>.Failure(UserAdministrationError.Conflict,
            "The user was changed by another request. Reload and try again.");
    private static UserAdministrationResult<T> LastAdministrator<T>() =>
        UserAdministrationResult<T>.Failure(UserAdministrationError.LastAdministrator,
            "The last active system administrator cannot be deactivated or stripped of the administrator role.");
    private static UserAdministrationResult<T> InvalidLanguage<T>() =>
        UserAdministrationResult<T>.Failure(UserAdministrationError.Validation,
            "Preferred language must be 'be' or 'en'.");
}
