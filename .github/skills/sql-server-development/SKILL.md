---
name: sql-server-development
description: "Design, implement, troubleshoot, or review Microsoft SQL Server persistence for .NET applications. Use for T-SQL, SQL Server schemas, indexes, constraints, execution plans, locking, transactions, EF Core SQL Server migrations, data migrations, and production-safe database changes. Do not use for database-agnostic work or another database provider."
compatibility: "Microsoft SQL Server or Azure SQL; this workspace uses EF Core SQL Server 8.0.11."
---

# SQL Server Development

Use this skill when a change depends on SQL Server behavior. Combine it with the repository's `entity-framework-core` and `erp-developer` skills when persistence changes implement an ERP business rule.

## Workflow

1. Inspect the active connection configuration, EF Core provider version, current model configuration, migrations, and applied migration state.
2. Identify the data invariant, expected row volume, access paths, transaction boundary, and concurrency behavior before changing the schema or query.
3. Prefer constraints and indexes that enforce business integrity in the database. Keep domain validation as well, but do not rely on application-only uniqueness or referential checks.
4. Use EF Core migrations for application-owned schema changes. Review generated SQL and edit the migration when existing data requires backfilling, staged nullability, or provider-specific SQL.
5. Make migrations safe for existing databases: add and populate data before making it required, use deterministic defaults, name constraints and indexes consistently, and avoid destructive operations unless explicitly requested.
6. For slow queries, capture the actual SQL and execution plan. Check estimates versus actual rows, scans, key lookups, implicit conversions, parameter sensitivity, spills, blocking, and missing or redundant indexes.
7. Keep transactions short. Define retry and idempotency behavior explicitly; do not blindly retry transactions with externally visible side effects.
8. Validate against SQL Server, not only an in-memory provider. Confirm migration application, rollback strategy, constraints, representative queries, and application startup.

## SQL Server Rules

- Use `decimal(p,s)` for money and quantities with precision chosen from the business range.
- Use `datetime2` or `datetimeoffset` deliberately; distinguish business dates from technical UTC timestamps.
- Use `rowversion` for optimistic concurrency where the aggregate requires lost-update protection.
- Use Unicode `nvarchar` for user-facing multilingual text and bounded lengths where the domain provides a limit.
- Create composite indexes in filter/join/order order supported by observed queries; use included columns only when they materially avoid lookups.
- Keep foreign keys trusted and indexed when joins or deletes depend on them.
- Prefer set-based, parameterized SQL. Avoid dynamic SQL unless identifiers or query shape genuinely require it; use `sp_executesql` with parameters.
- Treat isolation-level changes, lock hints, `NOLOCK`, triggers, cascading deletes, and online index operations as explicit design decisions with documented consequences.

## EF Core Migration Checks

- Compare `dotnet ef migrations list` with the target database before diagnosing an invalid-column or missing-object error.
- Generate an idempotent migration script for deployment environments where the application must not mutate the schema at startup.
- Check defaults for existing rows and remove temporary defaults when they are not part of the domain model.
- Verify both `Up` and `Down`; clearly state when rollback would lose data or cannot be made safe.
- Do not report a schema change complete until the intended database has the migration applied or the deployment script has been handed off explicitly.

## Completion Evidence

Report the affected database and migration, build/test status, applied-versus-pending migration state, and any operational risk such as table locking, backfill duration, or required maintenance window.
