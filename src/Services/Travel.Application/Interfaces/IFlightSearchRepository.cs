using FlightSearchAggregate = Travel.Domain.Aggregates.FlightSearch.FlightSearch;

namespace Travel.Application.Interfaces;

public interface IFlightSearchRepository
{
    Task<FlightSearchAggregate> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FlightSearchAggregate>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(FlightSearchAggregate search, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
