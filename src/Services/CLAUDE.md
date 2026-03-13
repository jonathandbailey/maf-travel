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
2. Create a vertical slice under `Features/{Domain}/Commands/` or `Features/{Domain}/Queries/`:
   - `{Operation}Command.cs` / `{Operation}Query.cs` — command/query record and handler **co-located in the same file**; command implements `IRequest<TResponse>`, handler implements `IRequestHandler<TCommand, TResponse>`
   - Shared `{Domain}Response.cs` at `Features/{Domain}/` — one response type for all operations on that domain
   - Optional `{Operation}Validator.cs` — extends `AbstractValidator<TCommand>`; `ValidationBehavior` skips if none registered
   - Handler publishes domain events via `IPublisher` after saving: iterate `aggregate.DomainEvents`, publish each, then call `aggregate.ClearDomainEvents()`; throws `NotFoundException` (from `Exceptions/`) if a required resource is not found

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

Bind endpoints to a local `*Request` record (not the Application command directly), and construct the command inside the handler. This keeps the API contract stable and prevents over-posting.

Endpoints contain no try/catch and no null checks — exceptions are mapped to RFC 7807 ProblemDetails responses by `GlobalExceptionHandler` (`Exceptions/GlobalExceptionHandler.cs`).

```csharp
public class TravelPlanEndpointMappings : IEndpoint
{
    public void MapRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/travel-plans");
        group.MapPost("", CreateAsync);
        group.MapGet("/{id:guid}", GetAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
    }

    private static async Task<IResult> CreateAsync(ISender sender, CreateTravelPlanRequest request)
    {
        var response = await sender.Send(new CreateTravelPlanCommand(
            request.Origin, request.Destination, request.NumberOfTravelers,
            request.StartDate, request.EndDate));
        return Results.Created($"/travel-plans/{response.Id}", response);
    }

    private static async Task<IResult> GetAsync(ISender sender, Guid id)
    {
        var response = await sender.Send(new GetTravelPlanQuery(id));
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> UpdateAsync(ISender sender, Guid id, UpdateTravelPlanRequest request)
    {
        var response = await sender.Send(new UpdateTravelPlanCommand(
            id, request.Origin, request.Destination, request.NumberOfTravelers,
            request.StartDate, request.EndDate));
        return Results.Ok(response);
    }
}

// Request records — co-located in the same Endpoints file
public record CreateTravelPlanRequest(string? Origin, string? Destination,
    int? NumberOfTravelers, DateTime? StartDate, DateTime? EndDate);

public record UpdateTravelPlanRequest(string? Origin, string? Destination,
    int? NumberOfTravelers, DateTime? StartDate, DateTime? EndDate);
```
