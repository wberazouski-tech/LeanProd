# EF Core migrations

LeanProd has two active EF Core contexts and one baseline migration for each:

| Store | Context | Provider | Migration directory |
| --- | --- | --- | --- |
| Internal | IdentityDbContext | SQLite | Common/Persistence/Migrations/Internal |
| Business | LeanProdDbContext | SQL Server | Common/Persistence/Migrations/Business |

The pre-split migrations remain in Common/Persistence/Migrations and are associated with LegacyLeanProdDbContext. Active contexts do not apply them. They are retained to verify and adopt existing installations.

Use -Store Internal for SQLite or -Store Business for SQL Server:

    ./scripts/add-migration.ps1 -Store Business -Name AddProductionOrders
    ./scripts/add-migration.ps1 -Store Internal -Name AddApplicationPreference
    ./scripts/validate-migrations.ps1
    ./scripts/update-database.ps1 -Store Internal
    ./scripts/update-database.ps1 -Store Business
    ./scripts/generate-migration-script.ps1 -Store Business

The internal SQLite file is created and migrated before the API accepts requests. SQL Server connection testing is separate from schema migration. A new ERP database is created explicitly by setup and receives ErpDatabaseInfo after BusinessInitialCreate.

Never edit either baseline after deployment. Add a new provider-specific migration. Destructive schema changes require an exact database target, verified backup and recovery plan.