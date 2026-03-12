using MediatR;
using Travel.Application.Features.TravelPlan;
using Travel.Application.Features.TravelPlan.Create;
using Travel.Application.Features.TravelPlan.Delete;
using Travel.Application.Features.TravelPlan.Get;
using Travel.Application.Features.TravelPlan.List;
using Travel.Application.Features.TravelPlan.Update;

namespace Travel.Api.Endpoints;

public class TravelPlanEndpointMappings : IEndpoint
{
    public void MapRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/travel-plans", CreateAsync);
        app.MapGet("/travel-plans", ListAsync);
        app.MapGet("/travel-plans/{id:guid}", GetAsync);
        app.MapPut("/travel-plans/{id:guid}", UpdateAsync);
        app.MapDelete("/travel-plans/{id:guid}", DeleteAsync);
    }

    private static async Task<IResult> CreateAsync(ISender sender, CreateTravelPlanCommand command)
    {
        var response = await sender.Send(command);
        return Results.Created($"/travel-plans/{response.Id}", response);
    }

    private static async Task<IResult> ListAsync(ISender sender)
    {
        var response = await sender.Send(new ListTravelPlansQuery());
        return Results.Ok(response);
    }

    private static async Task<IResult> GetAsync(ISender sender, Guid id)
    {
        var response = await sender.Send(new GetTravelPlanQuery(id));
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> UpdateAsync(ISender sender, Guid id, UpdateTravelPlanCommand command)
    {
        var response = await sender.Send(command with { Id = id });
        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteAsync(ISender sender, Guid id)
    {
        await sender.Send(new DeleteTravelPlanCommand(id));
        return Results.NoContent();
    }
}
