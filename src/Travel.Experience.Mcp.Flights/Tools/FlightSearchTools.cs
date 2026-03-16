using System.ComponentModel;
using ModelContextProtocol.Server;
using Travel.Experience.Mcp.Flights.Models;
using Travel.Experience.Mcp.Flights.Services;

namespace Travel.Experience.Mcp.Flights.Tools;

[McpServerToolType]
public class FlightSearchTools(IFlightSearchService flightSearch)
{
    [McpServerTool(Name = "search_flights")]
    [Description("Search for available flights between two locations. Returns flight options with departure/arrival times and price per person. For round trips, provide a return_date and results will include both outbound and inbound legs.")]
    public async Task<IEnumerable<FlightOption>> SearchFlightsAsync(
        string origin,
        string destination,
        string departureDate,
        string? returnDate = null,
        int passengers = 1,
        CancellationToken cancellationToken = default)
    {
        var departure = DateOnly.Parse(departureDate);
        var returnLeg = returnDate is not null ? DateOnly.Parse(returnDate) : (DateOnly?)null;

        var request = new FlightSearchRequest(origin, destination, departure, returnLeg, passengers);
        return await flightSearch.SearchAsync(request, cancellationToken);
    }
}
