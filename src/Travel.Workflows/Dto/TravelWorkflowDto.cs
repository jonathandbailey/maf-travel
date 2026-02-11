using Microsoft.Agents.AI.Workflows;

namespace Travel.Workflows.Dto;

public record TravelWorkflowRequest(string Message, CheckpointInfo? CheckpointInfo = null);