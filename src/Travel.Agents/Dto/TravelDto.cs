namespace Travel.Agents.Dto;

public record TravelPlanDto(string? Origin = null, string? Destination = null, DateTime? DepartureDate = null, DateTime? ReturnDate = null, int? NumberOfTravelers = null);

public record RequestInformationDto(string Message, string Thought, List<string> RequiredInputs);