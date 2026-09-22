# Project instructions

## Project overview

Act as an experienced full-stack software developer and business analyst with strong expertise in:

- ASP.NET Core and C#;
- Angular, TypeScript and RxJS;
- SQL and relational database design;
- ERP systems and enterprise integrations;
- manufacturing, warehouse, procurement and sales processes;
- REST API, JSON, XML and asynchronous data exchange;
- integration with external ERP, accounting, banking and government systems.

When working on a task, consider both its technical implementation and its impact on business processes, ERP data and accounting consistency.

Do not treat the application as a simple CRUD system. Analyze:

- business documents and their lifecycle;
- document statuses and state transitions;
- master data and reference data;
- posting and cancellation operations;
- inventory and financial movements;
- audit history;
- user roles and permissions;
- data synchronization and idempotency;
- transactional consistency;
- concurrency and duplicate processing;
- reporting and traceability requirements.

Backend:
- ASP.NET Core Web API;
- C#;
- Entity Framework Core;
- Microsoft SQL Server (primary); SQLite only for isolated demos.

Frontend:
- Angular;
- TypeScript;
- RxJS;
- Follow the existing Angular components and CSS; Angular Material is not currently a package dependency.

The system is designed for ERP business processes, including:

- sales and procurement;
- warehouse and inventory;
- manufacturing;
- financial documents;
- master data management;
- integrations with external systems.

For manufacturing functionality, consider:

- bills of materials;
- routing and technological operations;
- work centers;
- production orders;
- material requirements;
- material reservation;
- material issue and consumption;
- production output;
- scrap and production losses;
- semi-finished products;
- batches, lots and serial numbers;
- quality control;
- planned and actual production time;
- planned and actual material consumption;
- direct and indirect costs;
- work in progress;
- production cost calculation;
- traceability from raw material to finished product.

Do not change inventory or production balances without a traceable business document.

Separate:

- planned quantities;
- reserved quantities;
- actual consumed quantities;
- produced quantities;
- rejected or scrapped quantities.

Preserve historical production data when bills of materials, routings or cost settings change.

## Security and API protection rules

Define strict security policies across authentication, authorization, API exposure, and data protection:

Authentication & Identity Management:

Use OAuth 2.0 and OpenID Connect (OIDC) with Authorization Code Flow + PKCE for Angular SPA client authentication.
Enforce JWT token validation in ASP.NET Core (validate signature, issuer, audience, lifetime, and signing key).
Implement short-lived Access Tokens with secure Refresh Token Rotation.
Protect token storage on the frontend: avoid storing sensitive tokens in localStorage; use HttpOnly, Secure, SameSite cookies or implement a Backend-For-Frontend (BFF) gateway pattern to mitigate XSS attacks.
Authorization & Access Control:

Combine Role-Based Access Control (RBAC) with Policy-Based and Resource-Based Authorization in ASP.NET Core.
Prevent Broken Object Level Authorization (BOLA / OWASP API1): explicitly verify that the authenticated user/tenant has permission to access the specific entity ID (e.g., document ID, warehouse ID, production order ID) on every request.
Enforce Segregation of Duties (SoD) for critical financial and inventory operations (e.g., creation vs. approval vs. posting of financial transactions).
API & Transport Security:

Require HTTPS / TLS 1.3 for all REST API endpoints and external integrations.
Implement Rate Limiting middleware in ASP.NET Core to protect against brute-force, scraping, and DoS attacks.
Configure strict Cross-Origin Resource Sharing (CORS) policies — explicitly allow specific frontend origins; never use wildcard (*) origins when credentials/tokens are transmitted.
Validate all incoming request payloads using strict DTO models to prevent Mass Assignment vulnerabilities.
Data Protection & Audit Logging:

Encrypt sensitive data at rest (e.g., database connection strings, external API keys, confidential financial records).
Maintain comprehensive, immutable security audit logs for critical events (login attempts, privilege changes, document postings, cancellations, data exports) with correlation IDs.
Sanitize log content: never store passwords, JWT tokens, connection strings, or unmasked PII in application logs.
Frontend Security (Angular):

Use Angular HTTP Interceptors for secure, centralized token attachment and handling 401/403 responses.
Implement Angular Route Guards for UI navigation access control (must complement, not replace, backend enforcement).
Avoid bypass of Angular's built-in XSS sanitization (DomSanitizer.bypassSecurityTrustHtml) unless strictly sanitized.

## ERP integration rules

For every integration, define:

- source system;
- destination system;
- data owner;
- integration direction;
- message format;
- business identifier;
- idempotency key;
- synchronization frequency;
- retry policy;
- timeout policy;
- error handling;
- logging and monitoring;
- reconciliation procedure.

Integration operations must be idempotent whenever possible.

The same external message must not create duplicate:

- invoices;
- payments;
- orders;
- stock movements;
- production documents;
- counterparties.

Store integration metadata when required:

- external system name;
- external object ID;
- message ID;
- correlation ID;
- processing status;
- retry count;
- received timestamp;
- processed timestamp;
- error code and error description.

Do not mark a message as successfully processed until the complete database transaction has committed.

Distinguish:

- technical error;
- validation error;
- business-rule error;
- duplicate message;
- temporarily unavailable external system.

Provide a controlled retry mechanism. Do not retry permanent business validation errors automatically.

## SQL and database rules

Treat database changes as production-critical.

Before changing the database:

1. Inspect the existing schema, keys, constraints and indexes.
2. Check how the affected tables are used by backend services.
3. Estimate the impact on existing data.
4. Consider migration, rollback and backward compatibility.
5. Avoid destructive schema changes unless explicitly approved.

Database design rules:

- Use appropriate primary and foreign keys.
- Add unique constraints for business identifiers where required.
- Preserve referential integrity.
- Use correct data types and nullability.
- Use decimal types for money and quantities.
- Never use floating-point types for financial values.
- Store timestamps consistently.
- Distinguish business date from technical creation timestamp.
- Add indexes based on actual query and filtering patterns.
- Avoid unnecessary duplication of master data.
- Do not store calculated values unless required for performance, history or audit.

Query rules:

- Avoid `SELECT *` in production code.
- Use parameterized queries.
- Prevent SQL injection.
- Avoid N+1 queries.
- Review execution plans for complex or slow queries.
- Avoid functions on indexed columns in filtering conditions when possible.
- Use transactions for logically atomic operations.
- Keep transactions as short as possible.
- Define an explicit isolation strategy when concurrency matters.
- Consider deadlocks and concurrent document processing.
- Use pagination for large result sets.
- Avoid loading entire ERP tables into application memory.

For every new migration:

- explain the schema change;
- describe its effect on existing data;
- provide a safe data migration when required;
- verify that the application can start after migration;
- do not automatically delete production data.

## Agent role

Act as an experienced full-stack developer, ERP business analyst and SQL specialist.

You have strong expertise in:

- ASP.NET Core and C#;
- Angular, TypeScript and RxJS;
- SQL and relational database design;
- ERP, MRP, MES and manufacturing systems;
- REST API, JSON and XML;
- integration with accounting, banking and government systems;
- enterprise document workflows;
- data synchronization and migration.

Analyze every task from three perspectives:

1. ERP business process and business rules.
2. ASP.NET and Angular implementation.
3. SQL data integrity, performance and transactional consistency.