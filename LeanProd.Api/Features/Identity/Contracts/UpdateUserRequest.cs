using System;
using System.ComponentModel.DataAnnotations;

namespace LeanProd.Api.Features.Identity.Contracts;

public sealed record UpdateUserRequest(
    [Required, EmailAddress] string Email,
    [Required, MaxLength(200)] string DisplayName,
    [Required] string PreferredLanguage,
    Guid? DefaultDepartmentId,
    Guid? DefaultStorageLocationId,
    [Required] string ConcurrencyStamp);
