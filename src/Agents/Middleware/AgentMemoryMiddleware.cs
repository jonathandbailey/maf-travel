using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Agents.Extensions;
using Infrastructure.Repository;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Agents.Middleware;

public class AgentThreadMiddleware(IAgentThreadRepository repository, ILogger<IAgentAgUiMiddleware> logger) : IAgentThreadMiddleware, IAgentMiddleware
{
    private static readonly ActivitySource ActivitySource = new("Agents.Middleware", "1.0.0");

    public async Task<AgentResponse> RunAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var threadId = options.GetThreadId();

        using var activity = ActivitySource.StartActivity(
            $"middleware {Name}",
            ActivityKind.Internal,
            default(ActivityContext),
            [new("agent.thread_id", threadId)]);

        var memoryThread = await LoadAsync(innerAgent, threadId, activity);

        AgentResponse response;
        try
        {
            response = await innerAgent.RunAsync(messages, memoryThread, options, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to run agent for thread {ThreadId}", threadId);
            throw;
        }

        await PersistSessionAsync(innerAgent, memoryThread, threadId, activity, cancellationToken);

        return response;
    }

    public string Name { get; } = "agent-thread";

    public async IAsyncEnumerable<AgentResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var threadId = options.GetThreadId();

        using var activity = ActivitySource.StartActivity(
            $"middleware {Name} streaming",
            ActivityKind.Internal,
            default(ActivityContext),
            [new("agent.thread_id", threadId)]);

        var memoryThread = await LoadAsync(innerAgent, threadId, activity);

        IAsyncEnumerable<AgentResponseUpdate> stream;
        try
        {
            stream = innerAgent.RunStreamingAsync(messages, memoryThread, options, cancellationToken);
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

        await PersistSessionAsync(innerAgent, memoryThread, threadId, activity, cancellationToken);
    }

    private async Task<AgentSession> LoadAsync(AIAgent agent, Guid threadId, Activity? activity)
    {
        try
        {
            var json = await repository.LoadAsync(agent.Name!, threadId.ToString());
            var element = JsonDocument.Parse(json).RootElement;
            return await agent.DeserializeSessionAsync(element);
        }
        catch (FileNotFoundException)
        {
            return await agent.CreateSessionAsync();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to load thread session {ThreadId}, starting fresh", threadId);
            return await agent.CreateSessionAsync();
        }
    }

    private async Task PersistSessionAsync(AIAgent agent, AgentSession session, Guid threadId, Activity? activity, CancellationToken cancellationToken)
    {
        try
        {
            var threadState = await agent.SerializeSessionAsync(session, cancellationToken: cancellationToken);
            await repository.SaveAsync(agent.Name!, threadId.ToString(), threadState.GetRawText());
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Failed to persist thread session {ThreadId}", threadId);
            throw;
        }
    }
}
