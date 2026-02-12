using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace Travel.Workflows.Dto;

public record TravelWorkflowRequest(ChatMessage Message, Guid ThreadId, CheckpointInfo? CheckpointInfo = null, TravelPlanDto? TravelPlan = null);