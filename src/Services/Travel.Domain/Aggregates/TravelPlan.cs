using Travel.Domain.Events;

namespace Travel.Domain.Aggregates;

public class TravelPlan : AggregateRoot
{
    public string? Origin { get; private set; }
    public string? Destination { get; private set; }
    public int? NumberOfTravelers { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    private TravelPlan() { }

    public static TravelPlan Reconstitute(
        Guid id,
        string? origin,
        string? destination,
        int? numberOfTravelers,
        DateTime? startDate,
        DateTime? endDate) =>
        new()
        {
            Id = id,
            Origin = origin,
            Destination = destination,
            NumberOfTravelers = numberOfTravelers,
            StartDate = startDate,
            EndDate = endDate
        };

    public static TravelPlan Create(
        string? origin,
        string? destination,
        int? numberOfTravelers,
        DateTime? startDate,
        DateTime? endDate)
    {
        var plan = new TravelPlan
        {
            Origin = origin,
            Destination = destination,
            NumberOfTravelers = numberOfTravelers,
            StartDate = startDate,
            EndDate = endDate
        };

        plan.AddDomainEvent(new TravelPlanCreatedEvent(plan.Id));
        return plan;
    }

    public void Update(
        string? origin,
        string? destination,
        int? numberOfTravelers,
        DateTime? startDate,
        DateTime? endDate)
    {
        Origin = origin;
        Destination = destination;
        NumberOfTravelers = numberOfTravelers;
        StartDate = startDate;
        EndDate = endDate;

        AddDomainEvent(new TravelPlanUpdatedEvent(Id));
    }
}
