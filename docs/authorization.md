# Authorization

LeanProd uses ASP.NET Core Identity roles mapped to application permissions. Endpoints authorize permissions through dynamic policies; Angular permission checks are navigation/UI assistance only.

Permission constants and the role matrix are owned by `LeanProd.Application/Features/Identity`. API policy infrastructure is in `LeanProd.Api/Common/Authorization`.

Use a permission on an endpoint:

```csharp
[Authorize(Policy = Permissions.MasterDataManage)]
```

HTTP behavior is `401` for an anonymous request and `403` for an authenticated user without the permission. Resource rules such as ownership, department assignment, draft status, and open periods will use `IAuthorizationService.AuthorizeAsync(user, resource, operation)` when those aggregates are implemented.

Angular receives effective permissions from `login`, `register`, and `me`, exposes `auth.hasPermission(...)`, and provides `permissionGuard(...)`. Never rely on those client checks for security.

## Token session

- Access tokens are signed JWTs with a short lifetime and are stored only in Angular memory.
- Refresh tokens are random opaque values stored only in a `Secure`, `HttpOnly`, `SameSite=Strict` cookie.
- The database stores SHA-256 hashes, never raw refresh tokens.
- Every refresh revokes the presented token and issues a replacement in the same family.
- Reuse of a revoked token revokes every still-active token in that family.
- Angular retries one failed authorized request after a coordinated refresh. A page reload restores the session through the refresh cookie.

Local bootstrap administrator credentials are configured through .NET User Secrets and must not be committed. The seeded account receives the `SystemAdministrator` role.
