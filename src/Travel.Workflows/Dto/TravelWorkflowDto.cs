using Microsoft.Extensions.AI;
using Travel.Agents.Dto;

namespace Travel.Workflows.Dto;

public record TravelWorkflowRequest(
    ChatMessage Message,
    Guid ThreadId);

public record WorkflowRunRequest(
    ChatMessage Message,
    Guid ThreadId,
    TravelPlanState TravelPlan);
