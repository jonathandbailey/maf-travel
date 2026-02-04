namespace Travel.Agents.Planning.Dto;

public record TravelPlanDto(string? Origin, string? Destination, DateTime? StartDate, DateTime? EndDate, int? NumberOfTravelers);