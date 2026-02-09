namespace Travel.Workflows;

public class WorkflowException : Exception
{
    public WorkflowException()
    {
    }

    public WorkflowException(string message) : base(message)
    {
    }

    public WorkflowException(string message, Exception inner) : base(message, inner)
    {
    }
}