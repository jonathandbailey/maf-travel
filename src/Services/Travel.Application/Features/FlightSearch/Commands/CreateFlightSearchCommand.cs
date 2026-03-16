using MediatR;
using Travel.Application.Interfaces;
using FlightSearchAggregate = Travel.Domain.Aggregates.FlightSearch.FlightSearch;
using FlightOptionEntity = Travel.Domain.Aggregates.FlightSearch.FlightOption;

namespace Travel.Application.Features.FlightSearch.Commands;

public record FlightOptionData(
    string FlightNumber,
    string Airline,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    decimal PricePerPerson,
    int AvailableSeats);

public record CreateFlightSearchCommand(
    IReadOnlyList<FlightOptionData> FlightOptions) : IRequest<FlightSearchResponse>;

public class CreateFlightSearchCommandHandler(
    IFlightSearchRepository repository,
    IPublisher publisher) : IRequestHandler<CreateFlightSearchCommand, FlightSearchResponse>
{
    public async Task<FlightSearchResponse> Handle(CreateFlightSearchCommand command, CancellationToken cancellationToken)
    {
        var options = command.FlightOptions
            .Select(o => FlightOptionEntity.Create(
                o.FlightNumber,
                o.Airline,
                o.Origin,
                o.Destination,
                o.DepartureTime,
                o.ArrivalTime,
                o.PricePerPerson,
                o.AvailableSeats))
            .ToList();

        var search = FlightSearchAggregate.Create(options);

        await repository.AddAsync(search, cancellationToken);

        foreach (var domainEvent in search.DomainEvents)
            await publisher.Publish(domainEvent, cancellationToken);

        search.ClearDomainEvents();

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
