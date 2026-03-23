using FlightSearchAggregate = Travel.Domain.Aggregates.FlightSearch.FlightSearch;

namespace Travel.Application.Interfaces;

public interface IFlightSearchQuery
{
    Task<FlightSearchAggregate> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FlightSearchAggregate>> ListAsync(CancellationToken cancellationToken = default);
}
