using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

namespace Application.Workflows.Dto;

public class WorkflowStateDto(WorkflowState state, CheckpointInfo? checkpointInfo)
{
    public WorkflowState State { get; } = state;

    public CheckpointInfo? CheckpointInfo { get; } = checkpointInfo;
}

public class WorkflowResponse(WorkflowState state, string message)
{
    public WorkflowState State { get; } = state;
    public string Message { get; } = message;
}

public class StoreStateDto(CheckpointInfo checkpointInfo, JsonElement jsonElement)
{
    public CheckpointInfo CheckpointInfo { get; init; } = checkpointInfo;

    public JsonElement JsonElement { get; init; } = jsonElement;
}