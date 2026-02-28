using Agents.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Agents.Middleware;

public class AgentAgUiMiddleware(ILogger<IAgentAgUiMiddleware> logger) : IAgentMiddleware, IAgentAgUiMiddleware
{
    private static readonly ActivitySource ActivitySource = new("Agents.Middleware", "1.0.0");

    public async IAsyncEnumerable<AgentResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var threadId = options.GetAgUiThreadId();
        options = options.AddThreadId(threadId);

        using var activity = ActivitySource.StartActivity(
            $"middleware {Name} streaming",
            ActivityKind.Internal,
            default(ActivityContext),
            [new("ag_ui.thread_id", threadId)]);

        IAsyncEnumerable<AgentResponseUpdate> stream;
        try
        {
            stream = innerAgent.RunStreamingAsync(messages, thread, options, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to start streaming agent run for thread {ThreadId}", threadId);
            throw;
        }

        await foreach (var update in stream)
        {
            yield return update;
        }
    }

    public async Task<AgentResponse> RunAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var threadId = options.GetAgUiThreadId();
        options = options.AddThreadId(threadId);

        using var activity = ActivitySource.StartActivity(
            $"middleware {Name}",
            ActivityKind.Internal,
            default(ActivityContext),
            [new("ag_ui.thread_id", threadId)]);

        try
        {
            return await innerAgent.RunAsync(messages, thread, options, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to run agent for thread {ThreadId}", threadId);
            throw;
        }
    }

    public string Name { get; } = "agent-ag-ui";
}

public interface IAgentAgUiMiddleware
{
    IAsyncEnumerable<AgentResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken);
}