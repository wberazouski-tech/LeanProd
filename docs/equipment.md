# Equipment directory

## Scope

The equipment directory is a generic production master-data feature. Company-specific print types are explicitly outside the LeanProd template and are not implemented.

The user identifies equipment primarily by `Name` and, when available, `InventoryNumber`. There is no business `Code` field and no storage-location field.

## Equipment

The `Equipment` table contains:

- generated `Id`, hidden from users;
- required `Name`;
- optional `InventoryNumber`;
- optional `EquipmentTypeId`;
- required `DepartmentId`;
- optional `ParentEquipmentId` for line/machine/assembly hierarchies;
- optional `SerialNumber`, `Manufacturer`, `Model`, `CommissionedOn` and `Description`;
- `IsActive`, audit fields and `RowVersion`.

`InventoryNumber` is normalized by trimming whitespace and converting an empty string to `NULL`. A filtered unique index applies only to non-null values, so any number of assemblies may have no inventory number while two populated equal numbers are rejected.

Equipment display text is composed in Angular:

```text
<Name>, inv. no. <InventoryNumber>
```

If `InventoryNumber` is absent, only `Name` is rendered. No dash or empty inventory-number label is displayed. The rule is shared by the list, parent selector and hierarchy references.

`DepartmentId` and `EquipmentTypeId` must reference active records when saved. A parent must be active, cannot be the item itself and cannot create a cycle. An item with active children cannot be deactivated.

## User-defined equipment types

`EquipmentTypes` contains `Id`, unique `Name`, optional `Description`, `IsActive`, audit fields and `RowVersion`. No initial types are seeded. Users with `MasterData.Manage` create types only when their business requires them, and equipment may remain without a type.

A type referenced by active equipment cannot be deactivated.

## State history

Operational state is not overwritten on `Equipment`. Every change creates an `EquipmentStateEvents` record with:

- `EquipmentId`;
- `State`;
- `StartedAtUtc` and nullable `EndedAtUtc`;
- optional `Comment`;
- audit fields and `RowVersion`.

Supported state codes are `Operational`, `Maintenance`, `Repair`, `OutOfService` and `Decommissioned`; their UI names are localized in Belarusian and English.

Changing state is transactional and may be scheduled in the future for repair and maintenance planning. The service inserts the event into the chronological timeline: the preceding interval ends at the new start, while the new interval ends at the next already-planned event or remains open when it is last. This also permits an actual state to be inserted before an existing future plan without deleting that plan.

The current state is derived by the interval condition `StartedAtUtc <= now AND (EndedAtUtc IS NULL OR EndedAtUtc > now)`. Therefore, scheduling a future repair does not change the equipment's current state early. Future rows are highlighted and marked as planned in Angular.

Both dates form an editable interval. `EndedAtUtc` may be supplied immediately when a historical or planned event is created; it remains optional for an open-ended state. Users with `MasterData.Manage` can reopen any history row and change its state, start, end and comment. Editing uses `RowVersion` to prevent silent overwrites.

The server requires the end to be later than the start, rejects duplicate start timestamps and prevents an explicit end from crossing the following event's start. Gaps between intervals are allowed and mean that no equipment state is defined for that period. If no end is supplied and a later event exists, the interval automatically ends at the later event's start.

The `(EquipmentId, StartedAtUtc)` pair has a database-level unique index in addition to API validation.

The database enforces `EndedAtUtc > StartedAtUtc` and uses a filtered unique index on `EquipmentId WHERE EndedAtUtc IS NULL`. Consequently, each item has at most one current state even under concurrent requests.

## API and authorization

Equipment endpoints are under `/api/equipment`; type endpoints are under `/api/equipment-types`. The API supports paging and filters, CRUD-style create/update, activation, options, state history and state transitions.

Reading requires `MasterData.View`; mutation requires `MasterData.Manage`. Optimistic concurrency is applied to edited master-data records.

## Angular UI

The route `/master-data/equipment` provides:

- search and filters for department, optional type, current state and active status;
- a table centered on the formatted name/inventory identity;
- equipment create/edit dialog with an optional type and parent selector;
- empty-by-default user-managed equipment types;
- a state-transition dialog using local date/time converted to UTC for the API;
- optional end date during interval creation and an edit action on every history row;
- chronological state history with start, end and comment.

State history is sorted by `StartedAtUtc` in ascending order: the oldest interval is shown first and future plans follow at the bottom.
