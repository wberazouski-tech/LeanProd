# Feature folder convention

The backend uses the same primary business feature names in every relevant layer: `Identity`, `Organizations`, `MasterData`, `ShiftReports`, `Quality`, `PeriodClosing`, and `Reporting`.

Feature code lives under `Features/<Feature>`. Shared technical primitives live in `Common`. Inside Application, group code by use case (`Create`, `Update`, `GetById`) instead of global `Services`, `DTOs`, `Repositories`, or `Helpers` folders.

Only add a feature to a layer when that layer has a real responsibility for it. Placeholder `.gitkeep` files preserve the agreed initial module map and must be removed when the first implementation file is added.

The implemented `Organizations` feature is separate from `MasterData`: organization identity and registration data describe the system owner, while departments and storage locations remain operational master data. The shared `Address` aggregate belongs to `Organizations` and links to both master-data entities as possible owners.
