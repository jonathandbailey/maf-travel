using MediatR;
using Travel.Application.Features.FlightSearch;
using Travel.Application.Features.FlightSearch.Commands;
using Travel.Application.Features.FlightSearch.Queries;

namespace Travel.Api.Endpoints;

public class FlightSearchEndpointMappings : IEndpoint
{
    public void MapRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/flight-searches");
        group.MapPost("", CreateAsync);
        group.MapGet("", ListAsync);
        group.MapGet("/{id:guid}", GetAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
    }

    private static async Task<IResult> CreateAsync(ISender sender, CreateFlightSearchRequest request)
    {
        var response = await sender.Send(new CreateFlightSearchCommand(
            request.FlightOptions
                .Select(o => new FlightOptionData(
                    o.FlightNumber,
                    o.Airline,
                    o.Origin,
                    o.Destination,
                    o.DepartureTime,
                    o.ArrivalTime,
                    o.PricePerPerson,
                    o.AvailableSeats))
                .ToList()));
        return Results.Created($"api/flight-searches/{response.Id}", response);
    }

    private static async Task<IResult> ListAsync(ISender sender)
    {
        var response = await sender.Send(new ListFlightSearchesQuery());
        return Results.Ok(response);
    }

    private static async Task<IResult> GetAsync(ISender sender, Guid id)
    {
        var response = await sender.Send(new GetFlightSearchQuery(id));
        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteAsync(ISender sender, Guid id)
    {
        await sender.Send(new DeleteFlightSearchCommand(id));
        return Results.NoContent();
    }
}

public record FlightOptionRequest(
    string FlightNumber,
    string Airline,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    decimal PricePerPerson,
    int AvailableSeats);

public record CreateFlightSearchRequest(IReadOnlyList<FlightOptionRequest> FlightOptions);
