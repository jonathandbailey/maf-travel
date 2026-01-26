namespace Travel.Application.Api.Domain;

public class Session
{
    public Guid ThreadId { get; } = Guid.NewGuid();
    public Guid TravelPlanId { get; }

    public Session(Guid travelPlanId)
    {
        TravelPlanId = travelPlanId;
    }

    public Session(Guid threadId, Guid travelPlanId)
    {
        ThreadId = threadId;
        TravelPlanId = travelPlanId;
    }
}