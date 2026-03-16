using Travel.Workflows.Flights.Dto;

namespace Travel.Workflows.Interfaces;

public interface IFlightApiClient
{
    Task CreateFlightSearchAsync(IReadOnlyList<FlightOption> flights, CancellationToken cancellationToken = default);
}
