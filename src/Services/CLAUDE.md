# Services Layer (Clean Architecture)

This folder contains a clean architecture stack for the Travel domain. Each project has its own `CLAUDE.md` with detailed conventions.

## Projects

```
src/Services/
├── Travel.Domain/        # Aggregates, value objects, domain events
├── Travel.Application/   # CQRS use cases, repository interfaces, pipeline behaviors
├── Travel.Infrastructure/# Repository implementations, blob storage, document mapping
└── Travel.Api/           # Minimal API endpoints, IEndpoint registrations
```

**Dependency direction:** `Travel.Api` → `Travel.Application` → `Travel.Domain` ← `Travel.Infrastructure`

## Adding a new feature (end-to-end)

### Step 1 — Travel.Domain: model the aggregate

Add an aggregate root under `Aggregates/{Name}/`, inheriting `AggregateRoot`. Add value objects under `ValueObjects/` (inherit `ValueObject`). Raise domain events (implement `INotification`) via `AddDomainEvent()` inside aggregate methods.

### Step 2 — Travel.Application: define the contract and use case

1. Add a repository interface to `Interfaces/` (e.g. `ITravelPlanRepository.cs`)
2. Create a vertical slice under `Features/{Domain}/{Operation}/`:
   - `Command.cs` / `Query.cs` — implements `IRequest<TResponse>`
   - `Handler.cs` — implements `IRequestHandler<TCommand, TResponse>`; injects the repository interface; publishes domain events via `IPublisher` after saving
   - `Validator.cs` — extends `AbstractValidator<TCommand>`
   - `Response.cs` — plain record, no domain types

The `ValidationBehavior<TRequest, TResponse>` pipeline behavior in `Behaviors/` runs FluentValidation automatically before every handler.

### Step 3 — Travel.Infrastructure: implement persistence

1. Add an internal document type to `Documents/` (e.g. `TravelPlanDocument.cs`) — plain serializable record
2. Add a repository to `Repositories/` that:
   - Implements the interface from `Travel.Application.Interfaces`
   - Injects `IAzureStorageRepository` (from the shared `Infrastructure/` project)
   - Maps domain model ↔ document internally; never persists domain types directly
   - Serializes documents as JSON via `System.Text.Json`
3. Add an `InMemory{Name}Repository` in the same folder for test use
4. Register all repositories in `InfrastructureServiceCollectionExtensions.cs`

### Step 4 — Travel.Api: expose the endpoint

Add a class to `Endpoints/` implementing `IEndpoint`. Register routes in `MapRoutes()` and extract each handler to a `private static async Task<IResult>` method — never inline lambdas.

```csharp
public class TravelPlanEndpointMappings : IEndpoint
{
    public void MapRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/travel-plans", CreateAsync);
        app.MapGet("/travel-plans/{id}", GetAsync);
    }

    private static async Task<IResult> CreateAsync(ISender sender, CreateTravelPlanCommand cmd)
        => Results.Ok(await sender.Send(cmd));

    private static async Task<IResult> GetAsync(ISender sender, Guid id)
        => Results.Ok(await sender.Send(new GetTravelPlanQuery(id)));
}
```
