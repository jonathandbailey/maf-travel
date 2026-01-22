namespace Travel.Workflows.Models;


public enum TravelPlanStatus
{
    InProgress,
    Completed,
    Cancelled,
    NotStarted,
    Error
}

public enum FlightOptionsStatus
{
    Created,
    NotCreated
}

public enum UserFlightOptionsStatus
{
    Selected,
    UserChoiceRequired,
    NotSelected
}