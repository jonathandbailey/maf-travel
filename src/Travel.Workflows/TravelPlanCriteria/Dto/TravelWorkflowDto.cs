using Microsoft.Extensions.AI;
using Travel.Agents.Dto;

namespace Travel.Workflows.TravelPlanCriteria.Dto;

public record TravelWorkflowRequest(
    ChatMessage Message,
    Guid ThreadId);

public record WorkflowRunRequest(
    ChatMessage Message,
    Guid ThreadId,
    TravelPlanState TravelPlan);
