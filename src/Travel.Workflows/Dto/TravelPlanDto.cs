namespace Travel.Workflows.Dto;

public class TravelPlanDto(string? origin, string? destination, DateTimeOffset? startDate, DateTimeOffset? endDate)
{
    public string? Origin { get; } = origin;

    public string? Destination { get; } = destination;

    public DateTimeOffset? StartDate { get; } = startDate;

    public DateTimeOffset? EndDate { get; } = endDate;
}