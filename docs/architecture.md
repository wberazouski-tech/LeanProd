# LeanProd — target architecture

This specification was originally written for 1C. The web application preserves its business rules, not its platform-specific object model.

## 1C to ASP.NET Core / Angular mapping

| 1C concept | Web implementation |
| --- | --- |
| Catalog | Master-data aggregate exposed through REST API |
| Enumeration | C# enum mirrored by a generated/typed TypeScript union |
| Document | Transaction aggregate with draft/submitted/approved workflow |
| Information register | Effective-dated relational table |
| Accumulation register | Immutable transaction rows plus SQL/read-model aggregation |
| Posting a document | One database transaction that validates and persists the aggregate and its facts |
| Standard report variants | Server-side report queries with saved user filters/views |
| 1C roles | ASP.NET Core policy-based authorization |
| Record-level restriction | Query filters and resource authorization handlers |
| Data lock date | Server-side closed-period policy checked on every command |
| Additional attributes | Typed equipment/product parameters; JSON only for truly dynamic attributes |

## Modules

1. **Identity and Access** — users, roles, department assignments and resource policies.
2. **Master Data** — departments, units, print types, equipment, products, downtime reasons and effective performance standards.
3. **Shift Accounting** — shift-report aggregate containing production and linked downtime entries.
4. **Quality** — rejected-product reports and their product rows.
5. **Period Closing** — closed periods and centralized mutation guard.
6. **Reporting** — read-only projections for performance, downtime, waste, rejects and equipment KPIs.

## Architectural boundaries

The first release is a modular monolith. Each module owns its write model and exposes application commands/queries. Controllers must not contain formulas or direct EF Core queries. Cross-module changes run in a single database transaction. Reporting uses separate projections and must not mutate operational data.

Angular is organized by features (`core`, `shared`, `features/auth`, `features/master-data`, `features/shift-reports`, `features/quality`, `features/reports`, `features/admin`). Routes are lazy-loaded. API contracts are typed; components do not construct URLs or store server state directly.

## Non-negotiable business invariants

- Shift number is between 1 and 4.
- A production or downtime entry belongs to exactly one shift report.
- Downtime rows reference a production row through an internal identifier and are deleted with it.
- Products and downtime reasons are filtered by the selected equipment and effective date.
- Planned values are captured from effective standards when the report is submitted, so historical reports remain reproducible.
- Totals are calculated on the server and never trusted from the browser.
- Submitted records inside a closed period cannot be changed, including through bulk/API operations.
- Operator access is restricted to assigned departments and their own shift reports unless a policy grants broader access.

## Security baseline

- Secrets are supplied through environment variables or .NET user-secrets.
- Password verification uses PBKDF2 with a random salt and constant-time comparison. Migration to ASP.NET Core Identity is planned before production user management is enabled.
- Short-lived JWT access tokens are validated for signature and lifetime. Production requires issuer/audience validation and refresh-token rotation.
- CORS uses an explicit allow-list, HTTPS/HSTS is enabled, requests are rate-limited, and `/health` checks the database.
- Authorization is enforced in the API. Angular guards are user experience only.
- Uploaded files must use a private, validated storage adapter; the old Cloudinary demo is not part of the target domain.

## Delivery slices

1. Secure shell, identity and role policies.
2. Master-data CRUD and effective standards.
3. Shift-report draft workflow and formulas.
4. Submission, period closing and audit log.
5. Rejected-product workflow.
6. Report projections, exports and dashboards.
