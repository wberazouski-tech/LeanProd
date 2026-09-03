# Technologies and specifications

## Scope and ownership

`CatalogTechnology` is the production specification and routing aggregate. It is owned by the separate backend feature `Technologies`, not by `MasterData`:

```text
LeanProd.Domain/Technologies
LeanProd.Application/Features/Technologies
LeanProd.Infrastructure/Features/Technologies
LeanProd.Api/Features/Technologies
```

The aggregate references master data but does not own it. Referenced records include products and product classes, units of measure, departments, equipment and storage locations.

## Aggregate model

A technology targets exactly one active product or one active non-group product class. Its header contains code, name, version, validity period, default flag, required `Status`, description, active flag and `RowVersion`.

`Status` is stored as the `CatalogTechnologyStatus` enum: `InDevelopment` (Ў распрацоўцы), `Active` (Дзейнічае), or `NotUsed` (Не выкарыстоўваецца). New technologies are always created with `InDevelopment`; subsequent updates require an explicit valid status.

The aggregate contains:

- ordered stages, optionally based on reusable active stage templates;
- directed stage links with finish/start dependency type and non-negative lag;
- material norms with quantity, scrap percentage and consumption-tracking mode;
- ordered material supply-route steps between active storage locations or to a consumption point;
- stage outputs with receiving location and primary-output flag;
- operations with optional department/equipment, setup, run and labor time, and worker count.

Stage dependencies must reference stages in the same aggregate, cannot point to the same stage and cannot form a cycle. Quantities and worker counts are positive; durations, lag, lead time and scrap values are bounded by service and database validation.

## Defaults, versions and history

A technology can be assigned to a concrete product or to a product class. For either target, the database permits at most one active default technology. `VersionNo` is positive, and `ValidTo` cannot precede `ValidFrom`.

Technology updates use optimistic concurrency through `RowVersion`. The current implementation replaces the stored child graph in one explicit database transaction. Operational documents must eventually capture the selected technology version or a snapshot of its norms so later specification edits cannot rewrite production history.

## Codes

Technology, stage-template, stage and operation codes accept explicit values. When a code is blank, the infrastructure code generator supplies a four-character code. Database unique indexes remain the final duplicate guard. The current count-based candidate generation requires controlled retry/transaction hardening before it is considered safe for concurrent production creation.

## API and authorization

The public route remains `/api/catalog-technologies`:

```text
GET    /api/catalog-technologies
GET    /api/catalog-technologies/{id}
POST   /api/catalog-technologies
PUT    /api/catalog-technologies/{id}
POST   /api/catalog-technologies/{id}/activate
POST   /api/catalog-technologies/{id}/deactivate
GET    /api/catalog-technologies/stage-templates
POST   /api/catalog-technologies/stage-templates
PUT    /api/catalog-technologies/stage-templates/{id}
```

Reading requires `MasterData.View`; mutations require `MasterData.Manage`. These permission names are retained for API and frontend compatibility even though backend ownership is now `Technologies`.

## Persistence

Migration `20260828231256_AddCatalogTechnologies` introduced the schema. EF mappings remain in `LeanProd.Infrastructure/Features/Technologies/Persistence`, while migrations remain centrally in `Common/Persistence/Migrations`.

The namespace move does not rename tables or columns and does not require a database migration. Historical migration files remain immutable.

## Angular status

The existing screen and TypeScript contracts remain under `lean-prod-web/src/app/features/master-data/technologies` and `master-data.service.ts`. The backend extraction intentionally does not change the Web App interface; a frontend feature move can be performed separately.
