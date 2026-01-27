using Travel.Application.Domain.Flights;

namespace Travel.Application.Domain;

public class TravelPlan
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    public string? Origin { get; private set; }
    
    public string? Destination { get; private set; }
    
    public DateTimeOffset? StartDate { get; private set; }

    public DateTimeOffset? EndDate { get; private set; }

    public TravelPlanStatus TravelPlanStatus { get; private set; } = TravelPlanStatus.NotStarted;

    public FlightPlan FlightPlan { get; private set; } = new();

    public TravelPlan(){}

    public TravelPlan(
        Guid id, 
        string? origin, 
        string? destination, 
        DateTimeOffset? startDate, 
        DateTimeOffset? endDate, 
        TravelPlanStatus travelPlanStatus,
        FlightPlan flightPlan)
    {
        Id = id;
        
        Origin = origin;
        Destination = destination;
        StartDate = startDate;
        EndDate = endDate;

        TravelPlanStatus = travelPlanStatus;
        FlightPlan = flightPlan;
    }

    public void SetStartDate(DateTimeOffset startDate)
    {
        if (startDate != StartDate)
            StartDate = startDate;
    }
    public void SetEndDate(DateTimeOffset endDate)
    {
        if (endDate != EndDate)
            EndDate = endDate;
    }
    public void SetDestination(string destination)
    {
        ArgumentException.ThrowIfNullOrEmpty(destination);

        if (destination != Destination)
            Destination = destination;
    }

    public void SetOrigin(string origin)
    {
        ArgumentException.ThrowIfNullOrEmpty(origin);

        if (origin != Origin)
            Origin = origin;
    }

    public void AddFlightSearchOption(FlightOptionSearch flightOptions)
    {
        FlightPlan.AddFlightOptions(flightOptions);
    }

    public void SelectFlightOption(FlightOption flightOption)
    {
        FlightPlan.SelectFlightOption(flightOption);

        TravelPlanStatus = TravelPlanStatus.Completed;
    }
}