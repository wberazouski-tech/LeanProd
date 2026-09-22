---
name: erp-developer
description: "Design, implement, debug, or review ERP and manufacturing features with business-process integrity. Use for ASP.NET Core, C#, Angular, EF Core, SQL Server,  inventory, procurement, sales, warehouse, production, quality, accounting documents, integrations, synchronization, migrations, auditability, authorization, or reporting work."
argument-hint: "ERP feature, workflow, integration, or data change to implement or review"
compatibility: "Requires an ERP codebase; this workspace uses .NET 8, Angular, EF Core, and SQL Server."
---

# ERP Developer

Use this skill for changes where technical behavior must remain consistent with an ERP business process. Treat the system as a document and transaction platform, not as generic CRUD.

## When to Use

- implementing or changing sales, procurement, warehouse, inventory, manufacturing, quality, workforce, finance, or master-data behavior
- changing document statuses, posting, cancellation, reservation, consumption, production output, or reconciliation
- changing EF Core entities, queries, transactions, migrations, indexes, or database constraints
- adding external ERP, accounting, banking, government, or asynchronous message integrations
- changing Angular screens that create, approve, post, cancel, reconcile, or report business documents
- reviewing authorization, audit history, idempotency, concurrency, data integrity, or traceability

## Operating Principles

- Start from the business process and document lifecycle, then select the technical change.
- Preserve accounting, inventory, and production consistency across retries, failures, and concurrent requests.
- Never change inventory or production balances without a traceable business document.
- Preserve history when master data, bills of material, routings, prices, or cost settings change.
- Prefer the repository's existing layered structure, feature folders, domain abstractions, and validation patterns.
- Keep planned, reserved, actual, produced, rejected, and scrapped quantities distinct.
- Make assumptions explicit when requirements do not define ownership, timing, rounding, status transitions, or reversals.

## Workflow

### 1. Frame the business behavior

Identify:

- business actor, organization, site, warehouse, work center, or legal entity
- source document, resulting document, and document owner
- lifecycle states and permitted transitions
- master data and reference data involved
- quantities, units of measure, currencies, taxes, dates, and rounding rules
- posting, cancellation, reversal, approval, and correction behavior
- inventory, production, financial, quality, audit, and reporting effects
- authorization boundary and segregation-of-duties concerns

Write a short invariant statement before editing. Example: a posted material issue reduces available stock once, records its source production order, and can only be reversed by a compensating document.

### 2. Locate the controlling code path

Inspect only the nearest relevant surfaces first:

- feature folder, endpoint or command handler, domain entity/value object, and application abstraction
- EF Core configuration, `DbContext`, existing migration, and related indexes or constraints
- Angular route, component, form, service, and API contract when the UI is involved
- integration adapter, message envelope, background worker, and processing metadata when external systems are involved
- neighboring tests and existing implementations of the same lifecycle operation

Confirm which layer decides the behavior. Do not patch a controller, component, or DTO when a domain rule or application workflow owns the decision.

### 3. Choose the smallest coherent change

Preserve existing public contracts unless the business requirement requires a change. Keep business rules close to the domain/application workflow, persistence rules in EF Core configuration, and transport concerns at the API boundary.

For new behavior, define:

- command/query contract and validation
- authorization policy and organization/site scope
- state transition rules and failure responses
- transaction boundary and concurrency strategy
- audit event and traceability fields
- idempotency behavior for repeated requests
- read model or report implications

For a bug, reproduce it with the smallest business scenario and identify the violated invariant before changing code.

### 4. Protect data integrity

For database or persistence work:

1. Inspect existing keys, foreign keys, unique constraints, nullability, indexes, and provider behavior.
2. Use decimal types for money and quantities; never use floating-point values for financial data.
3. Distinguish business dates from technical timestamps and store timestamps consistently.
4. Keep logically atomic posting operations in one transaction and keep transactions short.
5. Define concurrency handling for competing reservations, postings, cancellations, and approvals.
6. Prevent duplicate business identifiers and repeated external messages with database-backed constraints where appropriate.
7. Treat migrations as production changes: explain existing-data impact, provide a safe data migration, preserve rollback options, and avoid destructive operations without explicit approval.

For queries, project only required fields, use `AsNoTracking` for read-only paths, avoid N+1 queries and unbounded loads, paginate large result sets, and inspect generated SQL or query plans when performance matters.

### 5. Handle integrations deliberately

For every integration, record or verify:

- source and destination system, data owner, direction, and message format
- stable business identifier, external object ID, message ID, and idempotency key
- synchronization frequency, timeout, retry policy, and backoff behavior
- technical, validation, business-rule, duplicate, and temporary-availability errors
- correlation ID, processing status, retry count, received/processed timestamps, and error details
- reconciliation procedure and operational visibility

Do not mark a message successful until the complete database transaction commits. Retry transient failures only; route permanent validation and business-rule failures to controlled handling.

### 6. Implement the user workflow

When Angular is involved:

- reflect the server's lifecycle and authorization rules in the UI without treating client validation as authoritative
- make status, posting, cancellation, approval, and error states explicit
- prevent accidental duplicate submits and show server-side validation clearly
- preserve identifiers, units, precision, dates, and totals without lossy conversions
- provide pagination, filtering, and traceability links for operational lists
- keep forms and tables consistent with existing Angular Material and RxJS patterns

### 7. Validate the business scenario

Add or update focused tests for:

- valid creation and each permitted state transition
- invalid transition, missing master data, authorization failure, and validation failure
- duplicate request or message processing
- concurrent reservation/posting where relevant
- transaction rollback and retry behavior
- inventory, production, financial, audit, and traceability effects
- API contract and Angular workflow behavior when the change crosses the boundary

Prefer domain unit tests for invariants, integration tests for EF Core transactions and database constraints, and endpoint/UI checks for the complete workflow.

### 8. Run completion checks

Before finishing, verify:

- the original invariant holds after success, failure, retry, and cancellation
- no balance changes occur without a source document and audit trail
- quantities, units, dates, currency, and rounding remain correct
- authorization and organization/site scoping are enforced server-side
- repeated requests are safe or explicitly rejected
- migrations apply safely and the application starts against the resulting schema
- API, frontend, integration, and reporting contracts remain compatible
- focused tests, build/type checks, and relevant smoke checks pass

Report assumptions, remaining risks, migration instructions, and any untested external-system behavior.

## Deliverable Shape

For an implementation task, deliver the code change, focused tests, and any migration or API documentation required by the repository. Summarize the business invariant protected, affected documents and balances, validation performed, and residual risks.

For a review task, list findings first, ordered by severity, with file references and concrete business impact. Then state assumptions, test gaps, and a brief change summary.
