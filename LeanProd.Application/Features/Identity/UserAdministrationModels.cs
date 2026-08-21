namespace LeanProd.Application.Features.Identity;

public sealed record UserListQuery(int Page, int PageSize, string? Search, bool? IsActive, string? Role);

public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalCount);

public sealed record UserSummary(
    Guid Id, string Email, string DisplayName, bool IsActive,
    string PreferredLanguage, Guid? DefaultDepartmentId, Guid? DefaultStorageLocationId,
    IReadOnlyCollection<string> Roles, DateTime CreatedAtUtc, DateTime? LastLoginAtUtc);

public sealed record UserDetails(
    Guid Id, string Email, string DisplayName, bool IsActive,
    string PreferredLanguage,
    Guid? DefaultDepartmentId, Guid? DefaultStorageLocationId,
    IReadOnlyCollection<string> Roles, IReadOnlyCollection<string> Permissions,
    DateTime CreatedAtUtc, DateTime? UpdatedAtUtc, DateTime? LastLoginAtUtc,
    string ConcurrencyStamp);

public sealed record RoleDetails(string Name, IReadOnlyCollection<string> Permissions);

public sealed record CreateUserCommand(
    string Email, string DisplayName, string TemporaryPassword, string PreferredLanguage,
    Guid? DefaultDepartmentId, Guid? DefaultStorageLocationId,
    IReadOnlyCollection<string> Roles);

public sealed record UpdateUserCommand(
    string Email, string DisplayName, string PreferredLanguage,
    Guid? DefaultDepartmentId, Guid? DefaultStorageLocationId, string ConcurrencyStamp);

public sealed record SetUserRolesCommand(
    IReadOnlyCollection<string> Roles, string ConcurrencyStamp);

public enum UserAdministrationError
{
    None,
    NotFound,
    Validation,
    Conflict,
    LastAdministrator,
    SelfDeactivation
}

public sealed record UserAdministrationResult<T>(
    T? Value, UserAdministrationError Error, string? Message = null,
    IReadOnlyCollection<string>? ValidationErrors = null)
{
    public bool Succeeded => Error == UserAdministrationError.None;
    public static UserAdministrationResult<T> Success(T value) => new(value, UserAdministrationError.None);
    public static UserAdministrationResult<T> Failure(
        UserAdministrationError error, string message, IReadOnlyCollection<string>? validationErrors = null) =>
        new(default, error, message, validationErrors);
}
