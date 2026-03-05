namespace Infrastructure.Repository.Entities;

public class TravelPlanEntity
{
    public Guid Id { get; set; }
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public int? NumberOfTravelers { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
