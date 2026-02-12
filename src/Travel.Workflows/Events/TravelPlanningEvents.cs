using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Dto;

namespace Travel.Workflows.Events;

public class TravelPlanUpdateEvent(TravelPlanDto travelPlanDto) : WorkflowEvent
{
    public TravelPlanDto TravelPlanDto { get; } = travelPlanDto;
}

public class TravelPlanningCompleteEvent() : WorkflowEvent
{
}