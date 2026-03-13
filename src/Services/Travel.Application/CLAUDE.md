# Travel.Application

Application layer — orchestrates use cases via MediatR. No infrastructure concerns.

## Patterns
- **Vertical slice with Commands/Queries grouping**: features live in `Features/{Domain}/Commands/` or `Features/{Domain}/Queries/`
- **Command and handler are co-located** in the same file (e.g. `CreateTravelPlanCommand.cs` contains both `CreateTravelPlanCommand` and `CreateTravelPlanCommandHandler`)
- **Shared response per domain**: `TravelPlanResponse.cs` lives at `Features/TravelPlan/` and is shared across all commands and queries — no per-operation response type
- **Pipeline behaviors** in `Behaviors/`: `ValidationBehavior<TRequest, TResponse>` runs FluentValidation before every handler; skips silently if no validators are registered
- **Domain events**: after saving, handlers iterate `aggregate.DomainEvents`, publish each via `IPublisher`, then call `aggregate.ClearDomainEvents()`
- Repository interfaces (e.g. `ITravelPlanRepository`) are defined here in `Interfaces/` and injected into handlers — no concrete infra types
- All repository methods accept `CancellationToken` and it is always passed through from the handler

## Folder structure
```
Features/
  TravelPlan/
    Commands/
      CreateTravelPlanCommand.cs   ← IRequest<TravelPlanResponse> + IRequestHandler (co-located)
      UpdateTravelPlanCommand.cs
      DeleteTravelPlanCommand.cs
    Queries/
      GetTravelPlanQuery.cs        ← IRequest<TravelPlanResponse> + IRequestHandler (co-located)
      ListTravelPlansQuery.cs      ← IRequest<IReadOnlyList<TravelPlanResponse>> + IRequestHandler
    TravelPlanResponse.cs            ← shared response record for all TravelPlan operations
Behaviors/
  ValidationBehavior.cs
Exceptions/
  NotFoundException.cs    ← thrown by handlers when a resource is not found
Interfaces/
  ITravelPlanRepository.cs   ← repository contracts
```

## Rules
- Reference Travel.Domain only (not Travel.Infrastructure)
- Validators use FluentValidation `AbstractValidator<T>`; none currently exist but the pipeline is ready
- Publish domain events after saving, not inside domain methods
- Responses are simple records/DTOs — no domain types leak to callers
- **Not-found**: `NotFoundException` (from `Exceptions/`) is thrown by the **repository**, not by the handler — handlers do not null-check repository results. `ITravelPlanRepository.GetAsync` returns `Task<TravelPlan>` (non-nullable); if the resource doesn't exist the repository throws before the handler receives a value. Never return `null` from a handler and never throw `KeyNotFoundException`
