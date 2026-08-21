using System.ComponentModel.DataAnnotations;

namespace LeanProd.Api.Features.Identity.Contracts;

public sealed record ResetUserPasswordRequest(
    [Required, MinLength(12)] string TemporaryPassword);
