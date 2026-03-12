# Travel.Domain

Pure domain layer — no infrastructure or application dependencies.

## Base classes (already implemented)
- **`BaseEntity`** — abstract base with `Guid Id` (protected init, defaults to `Guid.NewGuid()`)
- **`ValueObject`** — abstract base with structural equality via `GetEqualityComponents()`; implements `==`/`!=`
- **`AggregateRoot : BaseEntity`** — adds domain event collection; use `AddDomainEvent(INotification)` inside aggregate methods, `ClearDomainEvents()` after dispatch

## Patterns
- **Aggregate roots** inherit `AggregateRoot`
- **Entities** (non-root) inherit `BaseEntity`
- **Value objects** inherit `ValueObject` and override `GetEqualityComponents()`
- **Domain events** implement `MediatR.INotification` and are raised inside aggregate methods
- Repository interfaces live in **Travel.Application** (not here)

## Folder structure
```
Aggregates/{Name}/        ← aggregate root + child entities
ValueObjects/             ← shared value objects
Events/                   ← INotification domain events
```

## Rules
- No references to MediatR handlers, EF Core, Azure SDK, Application, or Infrastructure layers
- Domain logic lives on aggregate methods, not in services
- The existing `TravelPlanEntity` in the shared `Infrastructure/` project is the persistence model; `TravelPlan` here is the domain aggregate
