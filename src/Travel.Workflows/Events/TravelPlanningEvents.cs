using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Dto;

namespace Travel.Workflows.Events;

public class TravelPlanUpdateEvent(TravelPlanState travelPlanState) : WorkflowEvent
{
    public TravelPlanState TravelPlanState { get; } = travelPlanState;
}

public class TravelPlanningCompleteEvent(TravelPlanState travelPlan) : WorkflowEvent
{
    public TravelPlanState TravelPlan { get; } = travelPlan;
}

public class TravelPlanStatusUpdateEvent(string Status, string? Thought = null, string? Source = null) : WorkflowEvent
{
    public string Status { get; } = Status;
    public string? Thought { get; } = Thought;
    public string? Source { get; } = Source;
}