namespace Travel.Agents.A2A.Dto;

public record FlightSearchDto(string Origin, string Destination, DateTimeOffset DepartureDate, DateTimeOffset ReturnDate);