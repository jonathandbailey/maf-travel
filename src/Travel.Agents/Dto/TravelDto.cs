namespace Travel.Agents.Dto;

public record TravelPlanDto(string? Origin = null, string? Destination = null, DateTime? StartDate = null, DateTime? EndDate = null, int? NumberOfTravelers = null);

public record RequestInformationDto(string Message, string Thought, List<string> RequiredInputs);