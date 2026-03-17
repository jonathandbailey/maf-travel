using MediatR;
using Travel.Application.Interfaces;
using FlightSearchAggregate = Travel.Domain.Aggregates.FlightSearch.FlightSearch;

namespace Travel.Application.Features.FlightSearch.Queries;

public record ListFlightSearchesQuery : IRequest<IReadOnlyList<FlightSearchResponse>>;

public class ListFlightSearchesQueryHandler(
    IFlightSearchRepository repository) : IRequestHandler<ListFlightSearchesQuery, IReadOnlyList<FlightSearchResponse>>
{
    public async Task<IReadOnlyList<FlightSearchResponse>> Handle(ListFlightSearchesQuery query, CancellationToken cancellationToken)
    {
        var searches = await repository.ListAsync(cancellationToken);
        return searches.Select(ToResponse).ToList();
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
