# Database configuration

MS SQL Server is the primary LeanProd database. SQLite is supported only for isolated local demos; production and integration-test code must not assume SQLite behavior. CI integration tests use a disposable SQL Server Testcontainer.

## Local development

Store the local SQL Server connection outside Git:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\\OPTIMA;Database=LeanProd;User Id=sa;Password=<password>;Encrypt=True;TrustServerCertificate=True" --project LeanProd.Api/LeanProd.Api.csproj
```

`TrustServerCertificate=True` is restricted to the local developer instance. Deployed environments must use a trusted SQL Server certificate, a least-privilege application login, and their platform secret store. The `sa` account must not be used by deployed applications.

## Demo/test SQLite override

```json
{
  "Database": { "Provider": "Sqlite" },
  "ConnectionStrings": { "DefaultConnection": "Data Source=lean-prod.demo.db" }
}
```

EF Core migrations are owned by `LeanProd.Infrastructure`. SQL Server and SQLite have behavioral differences, so release migrations must always be validated against SQL Server.

## Organization data

The database contains one `Organization` row enforced by `CK_Organization_Singleton` (`Id = 1`). `Addresses` is a shared table for the organization, departments and storage locations. `CK_Addresses_OneOwner` requires exactly one populated owner foreign key. GLN values use a filtered unique index, so multiple null values are allowed but a populated GLN cannot be duplicated.

Addresses use restrictive foreign keys: an owner cannot be physically deleted while its addresses exist. The application normally deactivates master data instead of deleting it.

## Local bootstrap administrator

The development administrator is created only when both secrets are configured:

```powershell
dotnet user-secrets set "BootstrapAdmin:Email" "admin@leanprod.local" --project LeanProd.Api/LeanProd.Api.csproj
dotnet user-secrets set "BootstrapAdmin:Password" "<strong-local-password>" --project LeanProd.Api/LeanProd.Api.csproj
```

Remove or rotate the bootstrap password after creating managed production users.

## In-app SQL setup

The API now supports a setup-only mode. If no runtime connection string is
available from configuration or local encrypted storage, the API still starts
and exposes `/api/setup/status`.

After setup, the application SQL connection is stored locally at:

```text
%ProgramData%\LeanProd\database-settings.json
```

The app SQL password is encrypted with Windows DPAPI for the local machine. The
SQL administrator password is never stored; it is used only during connect/create
operations.

### Connect existing database

The administrator supplies SQL Server, database, SQL admin login/password and
the desired application login. The backend then:

1. Opens SQL Server with the admin connection and verifies that the database
   exists.
2. Verifies that this is a LeanProd database by checking `__EFMigrationsHistory`
   and core identity schema.
3. Reads EF Core migration history and compares it with the current application
   migrations.
4. If the database is current, it does not run migrations.
5. If the database is older, it returns `RequiresMigration` with the message
   `Database version is older than application version. Apply migrations?`.
   Migrations are applied only after the administrator confirms.
6. If the database is not LeanProd, it returns
   `Selected database is not a LeanProd database.` and does not modify it.
7. Creates or updates the SQL login/user for the application, grants
   `db_datareader` and `db_datawriter`, verifies the app connection, and stores
   the encrypted runtime connection string.

### Create new database

For a new database, the backend creates the SQL database with the admin
connection, runs all EF Core migrations, creates/updates the application
login/user, grants runtime permissions, verifies the app connection, and stores
the encrypted runtime connection string.

### Admin UI

The Angular app exposes:

- `/setup` for first-run setup or recovery when the runtime database connection
  is broken.
- `/administration/database-settings` for authenticated system administrators.

When the backend is not configured, the login page redirects to `/setup`.
