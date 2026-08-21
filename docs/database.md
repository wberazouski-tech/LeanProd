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
