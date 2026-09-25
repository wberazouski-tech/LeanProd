# Remove item property code

Status: agreed by the user on 2026-09-25.

Remove ItemPropertyDefinitions.Code from persistence, request/response contracts and the property form/list. Preserve Id-based value/option relationships, authorization, audit and RowVersion behavior. Sort definitions by name. Names are not a new unique key. Class and catalog item codes are unaffected.

Migration 20260925111423_RemoveItemPropertyCode drops Code and its class/code unique index, then creates a nonunique CatalogItemClassId index. Applied migrations remain unchanged.

Before applying, identify the exact target and take a verified backup. Original codes can only be recovered from that backup; Down throws rather than fabricating codes. Recovery plan: restore the backup into a separate database, verify it and deploy the previous application version. Replacing the working database requires separate authorization.

The local database initially had legacy migration history without BusinessInitialCreate. The user subsequently authorized updating it and removing the old history. The completed transition is documented below.

Verification: scripts/check.cmd passed (Release build, 51 domain tests, both EF model checks, lint, translation parity for 575 keys, 62 Angular tests, production build). Four ItemPropertyContractTests passed. The generated migration SQL passed on a separate temporary SQL Server database: two existing definitions with identical names, their Id/RowVersion and a choice option were preserved; creating a definition without Code succeeded. Testcontainers was not run because Docker is unavailable. Manual UI business acceptance was not performed.

Existing warnings: technology CSS size budget; EF CatalogTechnologyStatus sentinel.

## Applied to working database

On 2026-09-25, the user explicitly authorized the working database change and removal of legacy migration history. COPY_ONLY backup with CHECKSUM and VERIFYONLY: artifacts/backups/LeanProd_20260925_132231_e1e39276.json. Successfully restored to LeanProd_RestoreCheck_20260925_132253 and passed DBCC CHECKDB; the restore-check database is retained for recovery review.

Rehearsed baseline adoption, Code removal and deletion of the 23 exact known legacy history entries on the restored database. Compared all required business column types/nullability/precision against a fresh current SQL schema. Used a consistent isolated SQLite copy for read-only organization metadata; the working internal database was not passed to the adoption tool. The temporary SQLite copy and schema-reference database were removed afterwards.

Applied the same sequence to the working database using the existing --adopt-business mode, then the new EF migration. Created the required ErpDatabaseInfo metadata and retained these two history entries: 20260922113355_BusinessInitialCreate and 20260925111423_RemoveItemPropertyCode. Archived old history separately at artifacts/backups/LeanProd_20260925_132231_history.json. Historical migration source files and legacy Identity tables are retained.

Verification: all pre-existing table row counts match the restored backup, including 22 catalog items; Code is absent; DBCC CHECKDB passes; subsequent EF database update reports no pending migrations. Debug API build passed with zero warnings/errors; API restarted; /health/ready returns 200 and live OpenAPI schemas SavePropertyCommand/PropertyDefinitionDto contain no code property. No credentials, role assignments or active database configuration were changed.
