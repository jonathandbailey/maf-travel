namespace Travel.Agents.Shared.Dto;

public record FlightSearchDto(string Origin, string Destination, DateTimeOffset DepartureDate, DateTimeOffset ReturnDate);