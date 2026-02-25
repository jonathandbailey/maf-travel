using Travel.Workflows.Common;
using Travel.Workflows.Infrastructure;

namespace Travel.Tests.Shared;

public class InMemoryWorkflowSessionRepository : IWorkflowSessionRepository
{
    private readonly Dictionary<Guid, WorkflowSession> _store = new();

    public Task<WorkflowSession?> LoadAsync(Guid threadId)
    {
        _store.TryGetValue(threadId, out var session);
        return Task.FromResult(session);
    }

    public Task SaveAsync(WorkflowSession session)
    {
        _store[session.ThreadId] = session;
        return Task.CompletedTask;
    }
}
