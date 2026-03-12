# Travel.Api

ASP.NET Core Minimal API host. Routes delegate to MediatR commands/queries.

## Patterns
- **IEndpoint pattern**: define an `IEndpoint` interface with `MapRoutes(IEndpointRouteBuilder app)`
- Each feature area has one file in `Endpoints/` that registers its routes (e.g. `TravelPlanEndpointMappings.cs`)
- **Never inline route handlers** — always extract to a named `private static async Task<IResult>` method

## Example
```csharp
// TravelPlanEndpointMappings.cs
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

## Folder structure
```
Endpoints/
  TravelPlanEndpointMappings.cs
IEndpoint.cs
```

## IEndpoint registration (in Program.cs)
Scan the assembly for all `IEndpoint` implementations and call `MapRoutes` on each.

## Rules
- Reference Travel.Application only (not Travel.Domain or Travel.Infrastructure directly)
- Infrastructure and domain services are registered in their own layers and wired in Program.cs
- Return `Results<T1, T2>` typed results for OpenAPI correctness
- Validate request models via MediatR pipeline (not in endpoints)
