namespace Travel.Workflows.Dto;

public class TravelPlanDto
{
    public string? Origin { get; set; }

    public string? Destination { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }
}