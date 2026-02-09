namespace Travel.Workflows;

public enum WorkflowState
{
    Created,
    Executing, 
    Completed,
    Failed,
    Suspended
}