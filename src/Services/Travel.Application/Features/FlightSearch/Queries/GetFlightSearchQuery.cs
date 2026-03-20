using MediatR;
using Travel.Application.Interfaces;
using FlightSearchAggregate = Travel.Domain.Aggregates.FlightSearch.FlightSearch;

namespace Travel.Application.Features.FlightSearch.Queries;

public record GetFlightSearchQuery(Guid Id) : IRequest<FlightSearchResponse>;

public class GetFlightSearchQueryHandler(
    IFlightSearchQuery flightSearchQuery) : IRequestHandler<GetFlightSearchQuery, FlightSearchResponse>
{
    public async Task<FlightSearchResponse> Handle(GetFlightSearchQuery query, CancellationToken cancellationToken)
    {
        var search = await flightSearchQuery.GetAsync(query.Id, cancellationToken);
        return ToResponse(search);
    }

    private static FlightSearchResponse ToResponse(FlightSearchAggregate search) =>
        new(
            search.Id,
            search.CreatedAt,
            search.FlightOptions
                .Select(o => new FlightOptionResponse(
                    o.Id,
                    o.FlightNumber,
                    o.Airline,
                    o.Origin,
                    o.Destination,
                    o.DepartureTime,
                    o.ArrivalTime,
                    o.PricePerPerson,
                    o.AvailableSeats))
                .ToList());
}
