# LeanProd backend structure

The backend follows a pragmatic Clean Architecture dependency rule.

```text
LeanProd.Api ────────────────┐
    │                        ▼
    └── LeanProd.Application ◄── LeanProd.Infrastructure
                 │                        │
                 └──────► LeanProd.Domain ◄┘
```

## Projects

- `LeanProd.Domain` contains production entities, value objects, enums, domain services, invariants and domain events. It has no project dependencies.
- `LeanProd.Application` contains use cases, commands, queries, validation, DTOs and ports such as repositories, current-user access, clocks and `IUnitOfWork`. It depends only on Domain.
- `LeanProd.Infrastructure` implements Application ports using EF Core, SQL Server, ASP.NET Core Identity and external integrations. SQLite is limited to local demo tests. It depends on Application and Domain.
- `LeanProd.Api` is the HTTP host and composition root. It contains endpoints/controllers, middleware, authentication setup, authorization policies, OpenAPI, health checks and dependency registration. It depends on Application and Infrastructure.

Business rules must not be placed in controllers, EF configurations or Angular. Infrastructure types must not leak into Domain or Application.

## Implemented organization slice

- `LeanProd.Domain/Organizations` owns `Organization`, `Address` and fixed address-type codes.
- `LeanProd.Application/Features/Organizations` defines organization/address use cases and transport-neutral models.
- `LeanProd.Infrastructure/Features/Organizations` implements those use cases and contains EF Core mappings.
- `LeanProd.Api/Features/Organizations` exposes the singleton organization profile and shared-address endpoints.

`Organization` is a database-enforced singleton with key `1`. `Address` has exactly one owner: the organization, a department, or a storage location. This invariant is enforced by application logic and a SQL check constraint.
