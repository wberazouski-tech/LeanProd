namespace LeanProd.Application.Features.Identity;

public interface IUserAdministrationService
{
    Task<PagedResult<UserSummary>> GetUsersAsync(UserListQuery query, CancellationToken cancellationToken);
    Task<UserDetails?> GetUserAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<RoleDetails>> GetRolesAsync(CancellationToken cancellationToken);
    Task<UserAdministrationResult<UserDetails>> CreateUserAsync(
        CreateUserCommand command, Guid actorUserId, string traceId, CancellationToken cancellationToken);
    Task<UserAdministrationResult<UserDetails>> UpdateUserAsync(
        Guid id, UpdateUserCommand command, Guid actorUserId, string traceId, CancellationToken cancellationToken);
    Task<UserAdministrationResult<UserDetails>> SetRolesAsync(
        Guid id, SetUserRolesCommand command, Guid actorUserId, string traceId, CancellationToken cancellationToken);
    Task<UserAdministrationResult<bool>> SetActiveAsync(
        Guid id, bool isActive, Guid actorUserId, string traceId, CancellationToken cancellationToken);
    Task<UserAdministrationResult<bool>> ResetPasswordAsync(
        Guid id, string temporaryPassword, Guid actorUserId, string traceId, CancellationToken cancellationToken);
}
