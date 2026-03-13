# Travel.Api

ASP.NET Core Minimal API host. Routes delegate to MediatR commands/queries.

## Patterns
- **IEndpoint pattern**: define an `IEndpoint` interface with `MapRoutes(IEndpointRouteBuilder app)`
- Each feature area has one file in `Endpoints/` that registers its routes (e.g. `TravelPlanEndpointMappings.cs`)
- **Never inline route handlers** — always extract to a named `private static async Task<IResult>` method
- **Request DTOs at the API boundary** — endpoints bind to a local `*Request` record, not directly to Application commands. The endpoint constructs the command from the request + any route parameters.
- **Use `MapGroup` for route prefixes** — call `app.MapGroup("/base-path")` once and register all routes on the group; never repeat the base path on each `Map*` call.

## Why Request DTOs, not Commands directly

- Prevents over-posting: commands can gain internal fields (e.g. `CreatedBy`, `TenantId`) that callers must never set
- Keeps the API contract stable when command shapes evolve
- Eliminates `command with { Id = id }` hacks when route params and body params need merging
- OpenAPI schema reflects exactly what callers should send

Response types from the Application layer (`*Response` records) are used directly — no API-level response wrapper needed.

## Example
```csharp
// TravelPlanEndpointMappings.cs
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

// Request records — co-located in the same file or a Requests/ subfolder
public record CreateTravelPlanRequest(
    string? Origin,
    string? Destination,
    int? NumberOfTravelers,
    DateTime? StartDate,
    DateTime? EndDate);

public record UpdateTravelPlanRequest(
    string? Origin,
    string? Destination,
    int? NumberOfTravelers,
    DateTime? StartDate,
    DateTime? EndDate);
```

## Exception Handling

All error mapping is centralised — endpoints contain no try/catch and no null checks.

**Program.cs setup:**
```csharp
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

app.UseExceptionHandler();  // before route mapping
```

**`Exceptions/GlobalExceptionHandler.cs`** implements `IExceptionHandler` and maps:

| Exception | HTTP response |
|-----------|---------------|
| `FluentValidation.ValidationException` | 400 `Results.ValidationProblem()` with per-field errors from `exception.Errors` |
| `Travel.Application.Exceptions.NotFoundException` | 404 `Results.Problem(statusCode: 404)` |
| anything else | falls through → default 500 ProblemDetails |

All error responses follow RFC 7807 (ProblemDetails). Endpoints stay clean:
```csharp
// Good — handler throws NotFoundException; GlobalExceptionHandler maps it to 404
private static async Task<IResult> GetAsync(ISender sender, Guid id)
{
    var response = await sender.Send(new GetTravelPlanQuery(id));
    return Results.Ok(response);
}
```

## Folder structure
```
Endpoints/
  TravelPlanEndpointMappings.cs   ← mapping class + co-located Request records
Exceptions/
  GlobalExceptionHandler.cs       ← IExceptionHandler: maps exceptions to ProblemDetails
IEndpoint.cs
```

## IEndpoint registration (in Program.cs)
Scan the assembly for all `IEndpoint` implementations and call `MapRoutes` on each.

## Rules
- Reference Travel.Application only (not Travel.Domain or Travel.Infrastructure directly)
- Infrastructure and domain services are registered in their own layers and wired in Program.cs
- Return `Results<T1, T2>` typed results for OpenAPI correctness
- Validate request models via MediatR pipeline (not in endpoints)
- Never bind Application commands directly in endpoint method signatures
