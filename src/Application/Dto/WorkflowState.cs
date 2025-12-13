namespace Application.Dto;

public enum WorkflowState
{
    Initialized,
    Executing,
    WaitingForUserInput,
    Completed,
    Error
}