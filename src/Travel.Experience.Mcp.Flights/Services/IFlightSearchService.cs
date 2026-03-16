using Travel.Experience.Mcp.Flights.Models;

namespace Travel.Experience.Mcp.Flights.Services;

public interface IFlightSearchService
{
    Task<IEnumerable<FlightOption>> SearchAsync(FlightSearchRequest request, CancellationToken cancellationToken = default);
}
