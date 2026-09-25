# Internal SQLite and business SQL Server separation

Status: updated with the user on 2026-09-22.

LeanProd uses two independent databases. The embedded internal SQLite database owns ASP.NET Core Identity, users, roles, claims, refresh tokens, security audit, application settings and the installation organization profile. SQL Server owns ERP master and business data.

The application creates and migrates the SQLite file automatically. Its default location is %LOCALAPPDATA%\LeanProd\leanprod-internal.db; an installer may configure another writable persistent path. Docker and a separate database service are not required for the internal store.

AppUser default business references and auditable user IDs retain GUID values without database foreign keys. Services validate user defaults against SQL Server while it is available. No transaction spans SQLite and SQL Server.

New installations apply InternalInitialCreate to SQLite and BusinessInitialCreate to SQL Server. Every newly created ERP database contains one ErpDatabaseInfo row with a database ID, installation ID, organization snapshot, application version, schema version and creation timestamp.