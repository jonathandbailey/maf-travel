namespace Travel.Workflows.Exceptions;

public class WorkflowValidationException : Exception
{
    public WorkflowValidationException()
    {
    }

    public WorkflowValidationException(string message) : base(message)
    {
    }

    public WorkflowValidationException(string message, Exception inner) : base(message, inner)
    {
    }
}