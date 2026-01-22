using Travel.Workflows.Models;

namespace Travel.Workflows.Dto;

public class CreateFlightOptions(TravelPlanSummary travelPlan, ReasoningOutputDto message)
{
    public ReasoningOutputDto Message { get; } = message;
    public TravelPlanSummary TravelPlan => travelPlan;
}



public class AgentResponse(string source, string message, AgentResponseStatus status)
{
    public string Source { get; } = source;
    
    public string Message { get; } = message;

    public AgentResponseStatus Status { get; } = status;

    public override string ToString()
    {
        return $"{Source} : {Message}, Status: {Status}";
    }
}

public enum AgentResponseStatus
{
        Success,
        Error
}









