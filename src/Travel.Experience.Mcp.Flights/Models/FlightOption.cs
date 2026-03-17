namespace Travel.Experience.Mcp.Flights.Models;

public record FlightOption(
    string FlightNumber,
    string Airline,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    decimal PricePerPerson,
    int AvailableSeats);
