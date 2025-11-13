namespace Application.Workflows.ReAct.Dto;

public class WorkflowResponse(WorkflowState state, string message)
{
    public WorkflowState State { get; } = state;
    public string Message { get; } = message;
}
