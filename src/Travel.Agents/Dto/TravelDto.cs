namespace Travel.Agents.Dto;

public record TravelPlanDto(string? Origin = null, string? Destination = null, DateTime? StartDate = null, DateTime? EndDate = null, int? NumberOfTravelers = null);