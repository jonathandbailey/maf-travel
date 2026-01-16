namespace Travel.Workflows.Dto;

public enum WorkflowState
{
    Initialized,
    Executing,
    WaitingForUserInput,
    Completed,
    Error
}