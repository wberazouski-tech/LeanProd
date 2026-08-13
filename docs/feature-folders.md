# Feature folder convention

The backend uses the same primary business feature names in every layer: `Identity`, `MasterData`, `ShiftReports`, `Quality`, `PeriodClosing`, and `Reporting`.

Feature code lives under `Features/<Feature>`. Shared technical primitives live in `Common`. Inside Application, group code by use case (`Create`, `Update`, `GetById`) instead of global `Services`, `DTOs`, `Repositories`, or `Helpers` folders.

Only add a feature to a layer when that layer has a real responsibility for it. Placeholder `.gitkeep` files preserve the agreed initial module map and must be removed when the first implementation file is added.
