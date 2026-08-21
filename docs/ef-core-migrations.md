# EF Core migrations

## Responsibility and storage

`LeanProdDbContext` and all migrations belong to `LeanProd.Infrastructure`. Migration files are stored only in `Common/Persistence/Migrations`. Entity mappings are placed next to their feature (for example, `Features/Identity/Persistence`) and discovered through `ApplyConfigurationsFromAssembly`.

SQL Server is the production database provider. SQLite is intended only for isolated demo/test databases; do not use SQL Server migrations against SQLite.

## Local setup

Keep the SQL Server connection string in .NET User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<SQL Server connection string>" --project LeanProd.Api
dotnet tool restore
```

Alternatively, automation may set `ConnectionStrings__DefaultConnection`. Never commit a real password or connection string.

## Commands

Run commands from the repository root:

```powershell
./scripts/add-migration.ps1 -Name AddProductionOrders
./scripts/validate-migrations.ps1
./scripts/update-database.ps1
./scripts/generate-migration-script.ps1
```

To roll a local database back to a named migration:

```powershell
./scripts/update-database.ps1 -Migration InitialIdentity
```

Generated idempotent deployment SQL is written to `artifacts/migrations/LeanProd.sql`. The `artifacts` directory is ignored because the script must be regenerated from the reviewed migrations for each release.

## Runtime behavior

`Database:ApplyMigrationsOnStartup` is `true` only in `appsettings.Development.json`. It is `false` by default, so production instances never change their schema during application startup. A deployment pipeline must generate, review, back up, and then execute the idempotent SQL script before deploying the matching API version.

## Team rules

1. Change the entity and its `IEntityTypeConfiguration` together.
2. Create one migration with a descriptive name; inspect both `Up` and `Down`.
3. Run `validate-migrations.ps1` before committing.
4. Never edit an already deployed migration. Add a corrective migration instead.
5. Do not put data imports or secrets into migrations. Small deterministic reference-data changes are acceptable only after review.
6. Destructive operations (drop/rename/type narrowing) require an explicit backup and rollback plan.
7. Deploy migrations once per environment, before starting the new API version.

## Organization and address migration

Migration `20260820142343_AddOrganizationAndAddresses`:

- creates the singleton `Organization` table with an `Id = 1` check constraint;
- creates shared `Addresses` with exactly-one-owner, foreign-key and unique filtered GLN constraints;
- inserts the initial editable organization profile;
- copies populated legacy `StorageLocations.Address` values to primary `Delivery` addresses;
- drops the legacy column only after the data copy;
- restores primary storage addresses in `Down` before dropping the new tables.

This is a reviewed data-preserving migration. New installations receive the same schema and initial singleton organization record.

## Mapping conventions

New production entities must explicitly define required fields, maximum string lengths, decimal precision, indexes, delete behavior, and optimistic concurrency where concurrent updates are possible. Store timestamps as UTC. Feature-specific tables should use a stable schema once the module's first tables are introduced; changing schemas later requires a migration and deployment review.
