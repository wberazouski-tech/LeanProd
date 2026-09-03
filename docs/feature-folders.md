# Feature folder convention

The backend uses the same primary business feature names in every relevant layer: `Identity`, `Organizations`, `MasterData`, `Workforce`, `Technologies`, `ShiftReports`, `Quality`, `PeriodClosing`, and `Reporting`. `Setup` exists only in layers that participate in first-run database configuration.

Feature code lives under `Features/<Feature>`. Shared technical primitives live in `Common`. Inside Application, group code by use case (`Create`, `Update`, `GetById`) instead of global `Services`, `DTOs`, `Repositories`, or `Helpers` folders.

Only add a feature to a layer when that layer has a real responsibility for it. Placeholder `.gitkeep` files preserve the agreed initial module map and must be removed when the first implementation file is added.

The implemented `Organizations` feature is separate from `MasterData`: organization identity and registration data describe the system owner, while departments and storage locations remain operational master data. The shared `Address` aggregate belongs to `Organizations` and links to both master-data entities as possible owners.

`Workforce` owns employees, brigades and membership history. `Technologies` owns product specifications, stages, dependencies, material norms, supply routes, outputs, operations and stage templates. These features may reference master-data entities, but their domain models, application contracts, services, EF mappings and API controllers must not be placed back in `MasterData`.

The technology backend paths are:

```text
LeanProd.Domain/Technologies
LeanProd.Application/Features/Technologies
LeanProd.Infrastructure/Features/Technologies
LeanProd.Api/Features/Technologies
```

This convention does not require an immediate frontend move. The current Angular technology screen remains in `features/master-data/technologies` until its interface is refactored separately.
