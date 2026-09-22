# ERP task: <short business outcome>

Status: Draft / Ready / In progress / Review / Accepted
Owner: <developer>; business acceptance: <analyst>
Branch: <type/task-name>

## Problem and scope

Who needs this, what happens today, and what must happen after the change?
List affected modules and explicit exclusions. Link existing feature documentation.

## Business rules

- Actor and required permissions; behavior when access is denied.
- Document lifecycle: current state -> action -> resulting state; forbidden transitions.
- Required data and validation, including dates, units, precision and rounding.
- Inventory/financial impact: movements, reservations, posting and cancellation.
- Repeated request behavior and business/idempotency key.
- Concurrent edits: conflict detection and user-visible recovery.
- Audit information and historical values that must remain unchanged.

Use “not applicable” with a reason where a rule does not apply; do not invent business requirements.

## Acceptance examples

| Given | When | Then |
| --- | --- | --- |
| <starting data, actor and state> | <action> | <observable result and persisted effect> |
| <invalid data or forbidden role> | <same action> | <rejection; no partial changes> |
| <action already completed> | <retry> | <agreed repeat behavior> |

## Implementation and data impact

API contract, UI behavior and affected services. Migration/backfill needed? Existing data impact, backup and recovery plan if needed. Demo data required for acceptance. Link decisions; distinguish proposals from accepted rules.

## Verification and handoff

- Checks to run and evidence/results.
- Automated scenarios versus manual business acceptance.
- Remaining limitations or unresolved questions.
- Analyst's acceptance date and outcome (fill only after actual acceptance).
