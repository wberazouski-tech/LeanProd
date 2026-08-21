# Organization and addresses

LeanProd keeps exactly one organization in the `Organization` table. Its fixed key is `1`, enforced by a database check constraint. The UI opens a direct edit form without an organization list or selector.

The profile stores legal and trading names, legal form, ISO country code, three country-neutral registration identifiers (`TaxNumber`, `StatisticalNumber`, `CompanyRegistrationNumber`), default ISO currency, time zone, interface language, contact details and an optional print footer. UI labels can be adapted by country, for example NIP, REGON and KRS for Poland. Historical profile versions are intentionally not stored; audit fields and `RowVersion` protect the current record.

## Shared addresses

`Addresses` is shared by the organization, departments and storage locations. A database constraint requires exactly one owner. An address contains a fixed localized type, ISO country code, locality, postal code, one free-form address line, optional globally unique and check-digit validated GLN, and primary/active flags.

Each owner can have several addresses. Making an address primary clears the previous primary address for that owner. A primary address must be active. The migration converts old storage-location address values to primary delivery addresses before removing the old column.

## Access and API

- `Organization.View`: read the profile and organization addresses.
- `Organization.Manage`: edit the profile and add organization addresses.
- `MasterData.Manage`: maintain shared addresses and department/storage addresses.
- `GET/PUT /api/organization`
- `GET/POST /api/organization/addresses`
- `GET/POST /api/departments/{id}/addresses`
- `GET/POST /api/storage-locations/{id}/addresses`
- `PUT /api/addresses/{id}` plus activate, deactivate and make-primary actions.

Angular route `/administration/organization` contains the organization form and reusable address editor. The same editor is displayed for a selected department or storage location.
