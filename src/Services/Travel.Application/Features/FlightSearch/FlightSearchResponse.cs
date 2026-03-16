namespace Travel.Application.Features.FlightSearch;

public record FlightOptionResponse(
    Guid Id,
    string FlightNumber,
    string Airline,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    decimal PricePerPerson,
    int AvailableSeats);

public record FlightSearchResponse(
    Guid Id,
    DateTime CreatedAt,
    IReadOnlyList<FlightOptionResponse> FlightOptions);
