namespace Travel.Application.Domain;

public static class TravelPlanConstants
{
    public const string NotSet = "Not_Set";
}

public enum TravelPlanStatus
{
    InProgress,
    Completed,
    Cancelled,
    NotStarted,
    Error
}