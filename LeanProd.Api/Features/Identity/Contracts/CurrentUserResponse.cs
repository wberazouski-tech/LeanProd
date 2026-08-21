using System;

namespace LeanProd.Api.Features.Identity.Contracts;

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string PreferredLanguage,
    Guid? DefaultDepartmentId,
    Guid? DefaultStorageLocationId,
    string[] Roles,
    string[] Permissions,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc);
