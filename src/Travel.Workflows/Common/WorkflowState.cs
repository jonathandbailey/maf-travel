namespace Travel.Workflows.Common;

public enum WorkflowState
{
    Created,
    Executing, 
    Completed,
    Failed,
    Suspended,
    Resumed
}