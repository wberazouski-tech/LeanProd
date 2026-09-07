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

## Current feature migrations

- `20260823204817_AddCatalogItemCostHistory` adds effective catalog-item cost history.
- `20260823211506_SetCatalogItemCostScale` standardizes cost precision.
- `20260824182019_AddCatalogItemClasses` adds hierarchical product classes.
- `20260826213319_AddWorkforceAndBrigades` adds employees, brigades and membership history.
- `20260828231256_AddCatalogTechnologies` adds technologies, stage templates, stages, dependency links, materials and routes, outputs and operations.

### Technology-stage normalization

Migration `20260905194916_NormalizeTechnologyStagesAndTransitions` normalizes reusable production stages into `TechnologyStages`, preserves `CatalogTechnologyStages` as technology-specific usages, and replaces dependency links with `CatalogTechnologyStageTransitions`. It was applied to the local SQL Server database on 2026-09-05.

The migration is a data migration, not a rename: it backfills stage numbers as `LineNo * 10`, preserves child materials/outputs/operations on their existing usage rows, and discards legacy dependency type and lag because the new transition model has no such fields. The generated deployment SQL is `artifacts/NormalizeTechnologyStagesAndTransitions.sql`; it must still be reviewed and validated against a copy of every non-local SQL Server database before deployment.

### Stage department ownership

Migration `MoveStageDepartmentToTechnologyStage` moves `DepartmentId` from `CatalogTechnologyStages` to the reusable `TechnologyStages` directory. A legacy reusable stage used by more than one department is split into department-specific directory rows before the old column is removed; the technology-stage usages and all their materials, outputs and operations remain intact.

Moving technology CLR types from `MasterData` to `Technologies` is a code-ownership refactor only. Table names and relational mappings remain stable, so it must not create a schema migration. `dotnet ef migrations has-pending-model-changes` should report no changes after such a move.

## Mapping conventions

New production entities must explicitly define required fields, maximum string lengths, decimal precision, indexes, delete behavior, and optimistic concurrency where concurrent updates are possible. Store timestamps as UTC. Feature-specific tables should use a stable schema once the module's first tables are introduced; changing schemas later requires a migration and deployment review.
