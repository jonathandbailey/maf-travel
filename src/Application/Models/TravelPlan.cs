namespace Application.Models;

public class TravelPlan
{
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class TravelPlanSummary(TravelPlan plan)
{
    public bool HasOrigin { get; set; } = !string.IsNullOrEmpty(plan.Origin);
    public bool HasDestination { get; set; } = !string.IsNullOrEmpty(plan.Destination);
    public bool HasDates { get; set; } = plan.StartDate.HasValue && plan.EndDate.HasValue;
}



