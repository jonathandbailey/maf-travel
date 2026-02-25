namespace Travel.Workflows.Exceptions;

public class WorkflowValidationException : WorkflowException
{
    public WorkflowValidationException()
    {
    }

    public WorkflowValidationException(string message) : base(message)
    {
    }

    public WorkflowValidationException(string message, string nodeName, Guid threadId) : base(message, nodeName, threadId)
    {
    }
}
