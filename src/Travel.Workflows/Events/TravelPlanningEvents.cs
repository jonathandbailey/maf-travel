using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Dto;

namespace Travel.Workflows.Events;

public class TravelPlanUpdateEvent(TravelPlanDto travelPlanDto) : WorkflowEvent
{
    public TravelPlanDto TravelPlanDto { get; } = travelPlanDto;
}

public class TravelPlanningCompleteEvent(TravelPlanDto travelPlan) : WorkflowEvent
{
    public TravelPlanDto TravelPlan { get; } = travelPlan;
}

public class TravelPlanStatusUpdateEvent(string Status, string? Thought = null, string? Source = null) : WorkflowEvent
{
    public string Status { get; } = Status;
    public string? Thought { get; } = Thought;
    public string? Source { get; } = Source;
}