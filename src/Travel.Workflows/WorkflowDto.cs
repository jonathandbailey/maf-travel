using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Dto;

namespace Travel.Workflows;

public class WorkflowStateDto(WorkflowState state, CheckpointInfo? checkpointInfo)
{
    public WorkflowState State { get; } = state;

    public CheckpointInfo? CheckpointInfo { get; } = checkpointInfo;
}

public class WorkflowResponse(WorkflowState state, string message, WorkflowAction action)
{
    public WorkflowState State { get; } = state;

    public WorkflowAction Action { get; } = action;
    public string Message { get; } = message;
}

public class StoreStateDto(CheckpointInfo checkpointInfo, JsonElement jsonElement)
{
    public CheckpointInfo CheckpointInfo { get; init; } = checkpointInfo;

    public JsonElement JsonElement { get; init; } = jsonElement;
}