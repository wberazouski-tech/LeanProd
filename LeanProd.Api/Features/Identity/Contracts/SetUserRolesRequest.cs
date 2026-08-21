using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LeanProd.Api.Features.Identity.Contracts;

public sealed record SetUserRolesRequest(
    [Required] IReadOnlyCollection<string> Roles,
    [Required] string ConcurrencyStamp);
