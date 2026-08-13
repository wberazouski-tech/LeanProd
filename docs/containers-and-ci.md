# Windows development, containers, tests, and CI

## Current development workflow

LeanProd is currently developed by one owner together with Codex on Windows. Docker is not required for normal development. The default local stack is:

```text
Angular dev server -> ASP.NET Core API -> localhost\OPTIMA -> LeanProd
```

Run the API and Angular directly with the .NET and Node.js tooling. Use EF Core migrations against the local SQL Server configuration stored in User Secrets. Dockerfiles and Compose are retained only as an optional, deployment-ready alternative and do not alter this workflow.

## Optional Compose

This section is not needed for current Windows development. Use it later for CI/deployment verification or when a reproducible container environment becomes necessary. Requirements are Docker Desktop with Linux containers and Compose v2.

```powershell
Copy-Item .env.example .env
# Replace every placeholder in .env with a local strong secret.
docker compose config --quiet
docker compose up --build -d
docker compose ps
```

Open the web shell at `http://localhost:4200`. The API is also exposed at `http://localhost:5000`; readiness is `http://localhost:5000/health/ready`.

The stack contains SQL Server 2022 Express, the ASP.NET Core API, and Angular served by nginx. Startup is ordered by health checks. SQL data is persisted in the named `leanprod-sql-data` volume. Do not commit `.env`.

Stop containers without deleting data:

```powershell
docker compose down
```

Deleting the named volume permanently removes the container database and must be an explicit operator decision.

## Integration tests

The `LeanProd.Api.IntegrationTests` project uses Testcontainers to start a disposable real SQL Server. It validates migrations, liveness/readiness, Problem Details, login, Viewer and administrator policies, refresh-token rotation, and logout revocation.

```powershell
dotnet test tests/LeanProd.Api.IntegrationTests/LeanProd.Api.IntegrationTests.csproj
```

For now these tests run in GitHub CI, where Docker is available. They are not part of the normal local Windows workflow. Test credentials exist only in the test process/container and are not deployment credentials.

A future local mode may use the existing SQL Server instance with a strictly separate `LeanProd_IntegrationTests` database. It is intentionally not implemented yet: there is no current deployment need, and tests must never run against the development `LeanProd` database.

## CI gates

`.github/workflows/ci.yml` runs:

1. Backend restore, `dotnet format` verification, warnings-as-errors build, EF pending-model validation, and SQL Server Testcontainers integration tests.
2. Frontend deterministic install, ESLint, headless unit tests, and production build.
3. API and web image builds plus resolved Compose validation after code gates pass.

No CI secret is required for integration tests: all services are ephemeral on the isolated runner. Published environments must inject database, JWT, and bootstrap credentials from their secret manager.

The CI and container files are preparation for later deployment, not evidence of a current multi-developer workflow. They can remain unused locally without affecting API, Angular, SQL Server, migrations, or SQLTools.
