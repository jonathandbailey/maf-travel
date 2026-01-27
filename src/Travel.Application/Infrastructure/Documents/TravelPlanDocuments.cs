using Travel.Application.Domain;

namespace Travel.Application.Api.Infrastructure.Documents;

public class TravelPlanDocument
{
    public Guid Id { get; set; }
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public TravelPlanStatus TravelPlanStatus { get; set; }
    public FlightPlanDocument FlightPlan { get; set; } = new();
}

