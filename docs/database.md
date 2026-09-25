# Database configuration

LeanProd uses two independent databases. An embedded SQLite file stores users, roles, claims, refresh tokens, security audit, application settings and the installation organization profile. SQL Server stores ERP master and business data.

The SQLite database requires no server or password. The application creates and migrates %LOCALAPPDATA%\LeanProd\leanprod-internal.db automatically. Deployments may override the persistent file location with InternalDatabase:Path. Keep only the SQL Server connection string outside Git:

    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<SQL Server connection string>" --project LeanProd.Api

A new SQL Server ERP database is created by BusinessInitialCreate. Its singleton ErpDatabaseInfo row records the database ID, SQLite installation ID, organization snapshot, application version, schema version and creation time.

The editable ERP Organization and addresses remain in SQL Server because they participate in business documents and master data. SQLite keeps the installation profile used before an ERP database is attached and when new ERP databases are created.

AppUser.DefaultDepartmentId, AppUser.DefaultStorageLocationId, CreatedByUserId and UpdatedByUserId are GUID references without cross-database foreign keys. Application services validate defaults against SQL Server. Transactions never span SQLite and SQL Server.

See [database-split.md](database-split.md) for existing-installation export and adoption, and [ef-core-migrations.md](ef-core-migrations.md) for migration commands.