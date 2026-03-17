namespace Travel.Experience.Mcp.Flights.Models;

public record FlightSearchRequest(
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    DateOnly? ReturnDate,
    int Passengers);
