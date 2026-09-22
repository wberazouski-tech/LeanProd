# Local Windows development

Run commands from `D:\MyERP\LeanProd`. Reopen your terminal after installing tools so it receives the updated PATH.

## Installed tools

- .NET SDK 8.0.425 and local `dotnet-ef` 8.0.11.
- Node.js 20.20.2, npm 10.8.2, Angular CLI 17.3.11.
- Git for Windows.
- SQL Server Express, instance `.\SQLEXPRESS`, database `LeanProd`.

Angular 17.3 requires Node 18 or 20: https://angular.dev/reference/versions. These frontend versions are retained for compatibility with this project; upgrading Angular is a separate task.

## Start

Run these in two separate terminals (or open the CMD files):

```powershell
.\scripts\start-api.cmd
```

```powershell
.\scripts\start-web.cmd
```

- Web: https://localhost:4200
- API Swagger: https://localhost:5001/swagger
- Readiness: https://localhost:5001/health/ready

Press Ctrl+C in each terminal to stop its server. The CMD wrappers also work when PowerShell blocks `npm.ps1` under its default execution policy.

## Local credentials

The API uses Windows authentication to SQL Server under the current Windows account. The connection string, JWT signing key and bootstrap administrator credentials are in .NET User Secrets, outside the repository. Automatic startup migrations are disabled locally; apply migrations explicitly.

The local administrator email is `admin@leanprod.local`. To display its generated password **locally**, run:

```powershell
(Get-Content "$env:APPDATA\Microsoft\UserSecrets\LeanProd.Api-LocalDevelopment\secrets.json" -Raw | ConvertFrom-Json).'BootstrapAdmin:Password'
```

Do not commit or share the secrets file. A new local database does not contain business records from the previous laptop; those require a separate backup restore.

## Restore dependencies and apply future migrations

The `.config/` directory is local and ignored by Git. After a fresh clone, create the local tool manifest once before running checks or migration scripts:

```powershell
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 8.0.11
```

For an existing local manifest, use `dotnet tool restore`.

```powershell
dotnet tool restore
dotnet restore LeanProd.sln
npm.cmd --prefix lean-prod-web ci
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update --project LeanProd.Infrastructure --startup-project LeanProd.Api --context LeanProdDbContext
```

## Checks

```powershell
dotnet build LeanProd.sln -c Release
dotnet test tests/LeanProd.Domain.UnitTests/LeanProd.Domain.UnitTests.csproj -c Release
npm.cmd --prefix lean-prod-web run build
```

SQL Server Testcontainers integration tests require Docker. Run `scripts\check.cmd -Integration` locally; GitHub CI is no longer configured in the repository. See [containers-and-ci.md](containers-and-ci.md).

## HTTPS on another laptop

Create and trust a certificate on that machine, then export both files:

```powershell
dotnet dev-certs https --trust
dotnet dev-certs https --export-path lean-prod-web/ssl/localhost.pem --format Pem --no-password
Copy-Item lean-prod-web/ssl/localhost.key lean-prod-web/ssl/localhost-key.pem
```

Angular expects `localhost-key.pem`; the .NET export creates `localhost.key`. These certificate and key files are ignored by Git.
