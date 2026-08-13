using Microsoft.AspNetCore.Authorization;

namespace LeanProd.Api.Common.Authorization;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
