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

The target aggregate model contains:

- `TechnologyStages`, a reusable production-stage directory. A stage has its own stable code, name and department and may be used by many technologies;
- `CatalogTechnologyStages`, a technology-specific use of a directory stage. It has a positive, unique `StageNumber` within the technology (normally 10, 20, 30, ...), local duration, equipment, description, materials, outputs and operations;
- directed `CatalogTechnologyStageTransitions` between two uses of stages in the same technology. This allows both joins (`10 -> 30`, `20 -> 30`) and branches (`10 -> 20`, `10 -> 30`);
- material norms with quantity, scrap percentage and consumption-tracking mode;
- ordered material supply-route steps between active storage locations or to a consumption point;
- stage outputs with receiving location and primary-output flag;
- operations with optional department/equipment, setup, run and labor time, and worker count.

An `InDevelopment` technology may be saved without stages or materials so its header can be registered before the specification is complete. Other statuses require at least one complete stage. A transition must reference two different stage uses from the same technology and cannot form a cycle. A terminal stage has no outgoing transition; no synthetic `NextStageNumber = 0` is stored. Quantities and worker counts are positive; durations, lead time and scrap values are bounded by service and database validation.

## Defaults, versions and history

A technology can be assigned to a concrete product or to a product class. For either target, the database permits at most one active default technology. `VersionNo` is positive, and `ValidTo` cannot precede `ValidFrom`.

Technology updates use optimistic concurrency through `RowVersion`. The current implementation replaces the stored child graph in one explicit database transaction. Operational documents must eventually capture the selected technology version or a snapshot of its norms so later specification edits cannot rewrite production history.

## Codes

Technology, reusable stage, stage-template and operation codes accept explicit values. When a code is blank, the infrastructure code generator supplies a four-character code. Database unique indexes remain the final duplicate guard. The current count-based candidate generation requires controlled retry/transaction hardening before it is considered safe for concurrent production creation.

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
GET    /api/catalog-technologies/technology-stages?activeOnly=true
GET    /api/catalog-technologies/stage-duplicates?name={name}&departmentId={departmentId}
```

Reading requires `MasterData.View`; mutations require `MasterData.Manage`. These permission names are retained for API and frontend compatibility even though backend ownership is now `Technologies`.

## Persistence

Migration `20260828231256_AddCatalogTechnologies` introduced the original schema. Migration `20260905194916_NormalizeTechnologyStagesAndTransitions` was applied to the local SQL Server database. It:

1. create `TechnologyStages`;
2. backfill one directory row for every existing `CatalogTechnologyStages` row, retaining its code, name and description;
3. add `TechnologyStageId` and `StageNumber` to `CatalogTechnologyStages`, using `LineNo * 10` as the deterministic initial number;
4. copy old stage links to `CatalogTechnologyStageTransitions` and explicitly review non-default dependency types and non-zero lag, because the new model deliberately does not store them;
5. removes obsolete local stage columns and `CatalogTechnologyStageLinks` only after the copy.

For duplicated legacy stage codes, the migration assigns the backfilled directory stage the deterministic unique code `STG-<GUID>`; its display name and description are preserved. Legacy dependency type and lag are intentionally not carried to the simplified transition model. EF mappings remain in `LeanProd.Infrastructure/Features/Technologies/Persistence`, while migrations remain centrally in `Common/Persistence/Migrations`.

Migration `MoveStageDepartmentToTechnologyStage` moves the department relationship from the technology-specific usage to the reusable `TechnologyStages` directory. Historical migration files remain immutable. Where one legacy stage is used with different departments, the deployment migration creates a separate reusable stage for each department and repoints only the affected usages.

## Angular status

The screen remains under `lean-prod-web/src/app/features/master-data/technologies`. It displays the reusable stage in each technology-stage row, permits an explicit stage number and edits source/target transition pairs; dependency type and lag are no longer part of the routing model.
