namespace Travel.Agents.Dto;


public record RequestInformationDto(string Message, string Thought, List<string> RequiredInputs);

public class TravelPlanDto(
    string? origin = null,
    string? destination = null,
    DateTime? startDate = null,
    DateTime? endDate = null,
    int? numberOfTravelers = null)
{
    public string? Origin { get; set; } = origin;
    public string? Destination { get; set; } = destination;

    public int? NumberOfTravelers { get; set; } = numberOfTravelers;

    public DateTime? StartDate { get; set; } = startDate;

    public DateTime? EndDate { get; set; } = endDate;
}