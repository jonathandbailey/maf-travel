using System.Net.Http.Json;
using Travel.Workflows.Common.Interfaces;
using Travel.Workflows.Flights.Dto;

namespace Travel.Workflows.TravelPlanCriteria.Services;

public class FlightApiClient(HttpClient httpClient) : IFlightApiClient
{
    public async Task<Guid> CreateFlightSearchAsync(IReadOnlyList<FlightOption> flights, CancellationToken cancellationToken = default)
    {
        var request = new CreateFlightSearchRequest(
            flights.Select(f => new FlightOptionRequest(
                f.FlightNumber,
                f.Airline,
                f.Origin,
                f.Destination,
                f.DepartureTime,
                f.ArrivalTime,
                f.PricePerPerson,
                f.AvailableSeats))
            .ToList());

        var response = await httpClient.PostAsJsonAsync("api/flight-searches", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<FlightSearchApiResponse>(cancellationToken: cancellationToken);
        return result!.Id;
    }

    private record FlightSearchApiResponse(Guid Id, DateTime CreatedAt);

    private record FlightOptionRequest(
        string FlightNumber,
        string Airline,
        string Origin,
        string Destination,
        DateTime DepartureTime,
        DateTime ArrivalTime,
        decimal PricePerPerson,
        int AvailableSeats);

    private record CreateFlightSearchRequest(IReadOnlyList<FlightOptionRequest> FlightOptions);
}
