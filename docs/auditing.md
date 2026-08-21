# Entity auditing

Business entities that need ownership, change tracking, or concurrency protection inherit `AuditableEntity` from `LeanProd.Domain.Common`.

```csharp
public sealed class ShiftReport : AuditableEntity
{
    public Guid Id { get; set; }
}
```

The base class supplies:

- `CreatedAtUtc` and `CreatedByUserId`;
- `UpdatedAtUtc` and `UpdatedByUserId`;
- SQL Server `RowVersion` for optimistic concurrency.

`LeanProdDbContext` assigns audit values during `SaveChanges`/`SaveChangesAsync`. The API implementation of `ICurrentUser` reads the authenticated JWT name-identifier claim. Background jobs, migrations, and unauthenticated system work use a null actor; application code must not invent a user ID for those operations.

Creation fields are protected from later modification. All timestamps come from the injected `TimeProvider` and are stored in UTC. Controllers and handlers must not set audit values themselves.

`RowVersion` must be included in update commands once editable business entities are introduced. An EF Core `DbUpdateConcurrencyException` means the record changed since it was read and should normally become an HTTP 409 Problem Details response.

This metadata records the creator and latest editor only. It is not a full immutable audit history. Critical business transitions such as report submission and period closing will also need append-only audit events.

`Organization` and `Address` inherit `AuditableEntity`. The organization profile intentionally keeps only its current values and does not create historical profile versions. Address changes likewise use audit metadata and optimistic concurrency; activation is used instead of physical deletion.

Identity users and refresh tokens deliberately do not inherit `AuditableEntity`: authentication lifecycle data has separate security semantics.
