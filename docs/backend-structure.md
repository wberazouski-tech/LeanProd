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
- `LeanProd.Infrastructure` implements Application ports using EF Core, PostgreSQL, Identity, external integrations, exports and background jobs. It depends on Application and Domain.
- `LeanProd.Api` is the HTTP host and composition root. It contains endpoints/controllers, middleware, authentication setup, authorization policies, OpenAPI, health checks and dependency registration. It depends on Application and Infrastructure.

Business rules must not be placed in controllers, EF configurations or Angular. Infrastructure types must not leak into Domain or Application.
