using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeanProd.Api.Features.Identity.Contracts;

public sealed record CreateUserRequest(
    [Required, EmailAddress] string Email,
    [Required, MaxLength(200)] string DisplayName,
    [Required, MinLength(12)] string TemporaryPassword,
    string? PreferredLanguage,
    Guid? DefaultDepartmentId,
    Guid? DefaultStorageLocationId,
    IReadOnlyCollection<string> Roles);
