# Master-data consistency fixes

Status: implemented; business acceptance pending.

## Scope and invariant

Users with MasterData.View can inspect the directories and technology details.
Mutations require MasterData.Manage in the UI and the existing API policies.
A save must preserve the exact catalog cost and concurrency version, must not
submit twice while a request is pending, and must not apply an address response
to a different selected owner. Existing activation rules and audit/history
behavior remain owned by the backend. No inventory or financial postings are
introduced.

## Observable behavior

- Catalog costs use decimal strings in JSON responses and Angular forms. The
  server continues to use decimal(18,2); numeric request values remain accepted
  alongside strings. Consumers must handle the response cost as a string.
- Address owner changes cancel prior reads and clear the previous editor.
- Pending master-data mutations disable controls and reject duplicate submits.
  Failed saves release the lock so users can correct or retry them.
- Lists show loaded/total counts and explicitly load subsequent pages on demand.
  Sorting and tree selection apply to the loaded records; the UI states this
  scope. Technology reference lists can also request further catalog pages.
- Unit names and options reload on language change; pending local names are kept
  separately for each language.
- Technology details can be opened by viewers. Editing/copy actions use the
  selected technology ID, and section validation reports errors.
- Equipment date intervals require an end after the start.
- Required/invalid fields show inline feedback; list loading, empty and error
  states are visible, with retry actions on main lists.
- Selectable rows accept Enter/Space. Conditional dialogs use native modal
  behavior, labelled headings, Escape cancellation and focus restoration.

## Data and deployment impact

No migration, backfill or configuration change is required. Existing database
values and history are not rewritten. Deploy the cost response contract and
Angular client together. Tests of existing SQL Server workflows and manual
business acceptance are separate from the isolated JSON/UI checks.

## Verification

Regression coverage includes decimal JSON round trips, display precision,
duplicate requests, address-owner races, date validation, keyboard/modals,
loading beyond 5000 records, stale list responses, language switching and
read-only technology access. See tests under the master-data frontend directory
and CatalogCostJsonTests in the API test project.

## Technology editor navigation (2026-09-18)

- Clicking Technologies again returns the open editor to the list. Close and route changes use the same unsaved-change decision.
- Changed existing, new and copied records offer Save and leave, Discard changes, or Continue editing. An untouched new form can close immediately.
- Save and leave saves the header, stage structure, and changed materials/routes, outputs and operations across all stages through existing section endpoints. Requests run sequentially and preserve row versions.
- Section saves are not one database transaction. If a later request fails, completed saves remain persisted; the editor and remaining local changes stay open for correction/retry. Discard only discards changes that have not already been saved.
- Navigation and duplicate saves are blocked during saving. No database schema/configuration changes.
- Validation: production build, lint, translation parity (506 keys), and 39 frontend tests passed, including seven navigation/save regression tests. Existing component CSS budget warning remains. Live database save workflow was not exercised.

## Items page editor (2026-09-18)

- Item creation, copying and editing now replace the list with an inline page editor. Save, Close and status actions are in the top toolbar; class/group editors retain their existing dialogs.
- Save keeps the editor open and updates its baseline and row version. Items menu, active category tab, Close and navigation to other routes protect unsaved edits with Save and leave / Discard / Continue editing.
- Invalid input and failed requests keep the form open. Saving blocks duplicate submissions and navigation. Status changes are disabled while edits are pending.
- Exact decimal text for cost and existing permissions/API contracts remain intact. No database or configuration changes.
- Build, lint, translation parity (512 keys) and 46 frontend tests passed. The existing Technologies CSS budget warning remains. Live database saves and visual browser review were not performed.

## Equipment page editor (2026-09-18)

Equipment rows now open a separate card with its form and state history; the list and type sidebar are hidden. The Equipment menu and Close return to the list. Unsaved form changes offer Save and leave, Discard, or Continue editing, including route navigation. Normal Save keeps the card open. Read-only users can view the card/history; editing and copying require management permission in the UI and existing API authorization. State intervals and equipment types retain their existing dialogs and independent save operations.

Validation: build, lint, translation parity (518 keys), and 52 frontend tests passed. The pre-existing Technologies CSS budget warning remains. No database/configuration changes; live database saves and visual browser review were not performed.
