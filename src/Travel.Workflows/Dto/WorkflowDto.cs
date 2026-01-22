using System.Text.Json;
using Workflows;

namespace Travel.Workflows.Dto;



public class WorkflowResponse(WorkflowState state, string message, WorkflowAction action, JsonElement? payload = null)
{
    public WorkflowState State { get; } = state;

    public WorkflowAction Action { get; } = action;
    public JsonElement? Payload { get; } = payload;
    public string Message { get; } = message;
}

public class ArtifactCreated(Guid id, string key)
{
    public Guid Id { get; } = id;
    public string Key { get; } = key;
    public string Type { get; } = nameof(WorkflowAction.ArtifactCreated);
}
public class StatusUpdate(string source, string status, string details)
{
    public string Type { get; } = nameof(WorkflowAction.StatusUpdate);
    
    public string Source { get; } = source;

    public string Status { get; } = status;

    public string Details { get; } = details;
}



