using Travel.Workflows.Flights.Dto;

namespace Travel.Workflows.Common.Interfaces;

public interface IFlightApiClient
{
    Task<Guid> CreateFlightSearchAsync(IReadOnlyList<FlightOption> flights, CancellationToken cancellationToken = default);
}
