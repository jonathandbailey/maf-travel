using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Planning.Dto;

namespace Travel.Workflows.Planning.Events;

public class TravelPlanUpdateEvent(TravelPlanDto travelPlanDto) : WorkflowEvent
{
    public TravelPlanDto TravelPlanDto { get; } = travelPlanDto;
}