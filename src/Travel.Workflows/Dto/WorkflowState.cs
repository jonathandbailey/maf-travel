namespace Travel.Workflows.Dto;

public enum WorkflowState
{
    Initialized,
    Executing,
    WaitingForUserInput,
    Completed,
    Error
}

public enum WorkflowAction
{
    InputRequest,
    StatusUpdate,
    ArtifactCreated,
    ReportError,
    None
}