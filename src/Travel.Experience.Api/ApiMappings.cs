using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Travel.Experience.Api;

public static class ApiMappings
{
    public static WebApplication MapTravelPlanEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/travel/plans");

        group.MapGet("{id:guid}", async (
            Guid id,
            [FromServices] ITravelPlanRepository repository,
            CancellationToken cancellationToken) =>
        {
            var plan = await repository.GetAsync(id, cancellationToken);
            return plan is null ? Results.NotFound() : Results.Ok(plan);
        });

        return app;
    }
}