# API contract, errors, and logging

## OpenAPI

In Development, the API publishes:

- Swagger UI: `https://localhost:5001/swagger`
- OpenAPI document: `https://localhost:5001/swagger/v1/swagger.json`

The document declares the Bearer JWT security scheme. Swagger is intentionally disabled outside Development. Production exposure requires a separate authenticated operational endpoint or an exported document reviewed during deployment.

## Problem Details

API framework errors use `application/problem+json`. Validation failures include the standard `errors` dictionary. Every generated problem includes:

- `status` and `title`;
- request `instance`;
- `traceId` for correlation with server logs.

Unhandled exceptions return status 500. Exception messages are included only in Development; stack traces are never returned. New endpoints should use `Problem(...)`, `ValidationProblem(...)`, `NotFound()`, `Conflict()`, and standard status results rather than custom error envelopes.

## Structured logging

Serilog writes structured console events. The request completion event includes method, path, status code, elapsed time, trace ID, host, scheme, and authenticated user ID when available. HTTP 4xx events are warnings and 5xx/unhandled exceptions are errors.

Never log passwords, JWTs, refresh tokens, cookies, authorization headers, connection strings, or request/response bodies containing business or personal data. Use named message-template properties rather than string interpolation:

```csharp
logger.LogInformation("Shift report {ShiftReportId} submitted by {UserId}", reportId, userId);
```

The console sink is the initial local/deployment output. A centralized sink such as Seq or OpenTelemetry can be added later without changing application logging calls.

## Health checks

- `/health/live` confirms that the API process is running. It does not query external dependencies.
- `/health/ready` confirms that the API can reach its required SQL Server database.
- `/health` is a backwards-compatible alias for `/health/ready`.

Healthy checks return HTTP 200; degraded or unhealthy readiness returns HTTP 503. Responses contain only status, duration, and check names. They never expose exceptions, connection strings, SQL commands, or credentials.
