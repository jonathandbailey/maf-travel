namespace Application.Workflows.Conversations.Dto;

public class WorkflowResponse(WorkflowResponseState state, string message)
{
    public WorkflowResponseState State { get; } = state;
    public string Message { get; } = message;
}

public enum WorkflowResponseState
{
    UserInputRequired,
    Completed,
    Error
}