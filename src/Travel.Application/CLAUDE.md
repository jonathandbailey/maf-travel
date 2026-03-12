# Travel.Application

Application layer — orchestrates use cases via MediatR. No infrastructure concerns.

## Patterns
- **Vertical slice**: each feature lives in `Features/{Domain}/{Operation}/`
- Each slice contains: `Command.cs` or `Query.cs`, `Handler.cs`, `Validator.cs`, `Response.cs`
- **Pipeline behaviors** in `Behaviors/`: `ValidationBehavior<TRequest, TResponse>` runs FluentValidation before every handler
- **Domain events**: handlers publish `INotification` events via `IPublisher` after the main operation
- Repository interfaces (e.g. `ITravelPlanRepository`) are defined here in `Interfaces/` and injected into handlers — no concrete infra types

## Folder structure
```
Features/
  TravelPlan/
    Create/
      Command.cs          ← IRequest<Response>
      Handler.cs          ← IRequestHandler
      Validator.cs        ← AbstractValidator<Command>
      Response.cs
    Get/
      Query.cs
      Handler.cs
      Response.cs
Behaviors/
  ValidationBehavior.cs
Interfaces/
  ITravelPlanRepository.cs   ← repository contracts
```

## Rules
- Reference Travel.Domain only (not Travel.Infrastructure)
- Validators use FluentValidation `AbstractValidator<T>`
- Publish domain events after saving, not inside domain methods
- Responses are simple records/DTOs — no domain types leak to callers
