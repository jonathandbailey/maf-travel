namespace Travel.Workflows.Exceptions;

public class WorkflowException : Exception
{
    public string? NodeName { get; }
    public Guid? ThreadId { get; }

    public WorkflowException()
    {
    }

    public WorkflowException(string message) : base(message)
    {
    }

    public WorkflowException(string message, Exception inner) : base(message, inner)
    {
    }

    public WorkflowException(string message, string nodeName, Guid threadId) : base(message)
    {
        NodeName = nodeName;
        ThreadId = threadId;
    }

    public WorkflowException(string message, string nodeName, Guid threadId, Exception inner) : base(message, inner)
    {
        NodeName = nodeName;
        ThreadId = threadId;
    }
}
