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
        var group = app.MapGroup("/travel-plans");
        group.MapPost("", CreateAsync);
        group.MapGet("", ListAsync);
        group.MapGet("/{id:guid}", GetAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
    }

    private static async Task<IResult> CreateAsync(ISender sender, CreateTravelPlanRequest request)
    {
        var response = await sender.Send(new CreateTravelPlanCommand(
            request.Origin, request.Destination, request.NumberOfTravelers,
            request.StartDate, request.EndDate));
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
        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateAsync(ISender sender, Guid id, UpdateTravelPlanRequest request)
    {
        var response = await sender.Send(new UpdateTravelPlanCommand(
            id, request.Origin, request.Destination, request.NumberOfTravelers,
            request.StartDate, request.EndDate));
        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteAsync(ISender sender, Guid id)
    {
        await sender.Send(new DeleteTravelPlanCommand(id));
        return Results.NoContent();
    }
}

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
