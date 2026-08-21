# Units of measure

## Decision

LeanProd uses the versioned `international-units.catalog.json` file as the only source from which an administrator may add a unit. The file is embedded in `LeanProd.Infrastructure`; it contains the classifier code, English name, international symbol and international letter code.

When a unit is selected, these four values are copied into SQL Server. This keeps operational records stable if the catalog file changes later and allows database queries without reading JSON. Users cannot type or edit international values manually.

## Database model

`UnitOfMeasures` contains:

- `Id` — internal generated `uniqueidentifier`, hidden from users;
- `Code` — classifier code, unique;
- `Name` — English name of the unit;
- `Symbol` — nullable international symbol;
- `LetterCode` — international letter code, unique;
- `QuantityType` — `Mass`, `Length`, `Area`, `Volume`, `Time`, `Temperature`, `Count` or `Other`;
- `DecimalPlaces` — allowed display/input precision from 0 to 6;
- `IsActive` — whether the unit can be used in new operations;
- `RowVersion` — optimistic concurrency protection.

The rarely changed directory intentionally has no `CreatedAtUtc`, `CreatedByUserId`, `UpdatedAtUtc` or `UpdatedByUserId` fields.

`UnitOfMeasureTranslations` has the composite key `(UnitOfMeasureId, LanguageCode)` and stores a local name. The English name is already stored on the main record, so no `en` translation is required. For a Belarusian interface, a Belarusian name is required when adding or editing a unit. A missing translation falls back to the English name.

## Conversions

`UnitOfMeasureConversions` stores one directed rule with `FromUnitId`, `ToUnitId`, `Multiplier decimal(28,12)`, `Offset decimal(28,12)` and `RowVersion`.

```text
result = source × multiplier + offset
reverse = (result - offset) / multiplier
```

Example: tonne to kilogram has `Multiplier = 1000` and `Offset = 0`, therefore `2 t = 2000 kg`.

Rules are accepted only for two different active units with the same `QuantityType`. The multiplier must be greater than zero. A direct or reverse duplicate is rejected. A unit participating in a conversion cannot be deactivated until its conversion rules are removed.

## API and UI

The API is exposed under `/api/unit-of-measures` and uses the existing `MasterData.View` and `MasterData.Manage` policies. It supports catalog search, list/details, create/update, activation and conversion management.

`POST /api/unit-of-measures/conversions/calculate` performs direct or reverse conversion and rounds the result to the target unit's `DecimalPlaces` using midpoint-away-from-zero rounding.

The Angular screen `/master-data/units-of-measure` provides a searchable units table, modal catalog picker, local-name input, conversion dialog and configured-rules table. The server revalidates catalog membership and all conversion invariants.

### Interactive conversion explanation

The text below the conversion table changes when the pointer is placed over a conversion row. For a proportional rule without an offset, the UI displays the relationship in a business-readable form:

```text
1 <to unit> = <multiplier> <from unit>
```

Example for a row `metre → kilometre` with coefficient `1000`:

```text
1 kilometre (KMT) = 1000 metre (MTR)
```

Both the English unit name and international letter code are shown so that similarly named units can be distinguished. When the pointer leaves the row, the text returns to the general instruction telling the user to hover over a rule.

If `Offset` is not zero, a simple ratio would be misleading. In that case the UI shows the complete formula with the source unit, target unit, multiplier and offset. The hover behaviour is implemented entirely in Angular and does not issue additional API requests.
