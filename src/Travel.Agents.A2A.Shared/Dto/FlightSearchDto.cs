namespace Travel.Agents.A2A.Shared.Dto;

public record FlightSearchDto(string Origin, string Destination, DateTimeOffset DepartureDate, DateTimeOffset ReturnDate);