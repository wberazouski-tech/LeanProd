# SQLite internal database and SQL Server ERP database

## Ownership

The embedded SQLite file stores users, roles, claims, external logins, Identity tokens, refresh tokens, security audit, application settings and the installation organization profile. SQL Server stores ERP data.

New installations use InternalInitialCreate for SQLite and BusinessInitialCreate for SQL Server.

At startup the application creates the SQLite directory and file when they do not exist, then applies internal migrations before accepting requests. By default the file is %LOCALAPPDATA%\LeanProd\leanprod-internal.db. An installer may set InternalDatabase:Path to another writable persistent location. SQL Server configuration and migration remain independent.

## Existing installation migration

Stop the API so users and refresh tokens cannot change during export. Set the source connection and destination file only in the current process:

    $env:LEANPROD_LEGACY_SQL_CONNECTION = '<legacy SQL Server connection string>'
    $env:LEANPROD_INTERNAL_SQLITE_PATH = 'C:\ProgramData\LeanProd\leanprod-internal.db'
    dotnet run --project tools/LeanProd.IdentityMigration -c Release

The exporter refuses to overwrite a non-empty Identity store. It copies users with password hashes and stable GUIDs, roles, assignments, claims, external logins, tokens, refresh tokens, security audit and the organization profile in one SQLite transaction.

After verifying login, roles and row counts, create and test a SQL Server backup. Then adopt the existing ERP schema:

    $env:LEANPROD_BACKUP_CONFIRMED = 'true'
    dotnet run --project tools/LeanProd.IdentityMigration -c Release -- --adopt-business

Adoption verifies all 23 legacy migrations, creates ErpDatabaseInfo, and records BusinessInitialCreate in __EFMigrationsHistory. It does not delete ERP data or old Identity tables. Clear the environment variables after completion.