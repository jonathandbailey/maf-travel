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
    public async Task<AgentResponse> RunAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken)
    {
        if (options == null)
        {
            logger.LogError("AgentRunOptions is null. AG-UI middleware requires an Agent Run Options.");
            throw new ArgumentException("AgentRunOptions is null");
        }

        var threadId = options.GetThreadId();

        var memoryThread = await LoadAsync(innerAgent, threadId);

        var response = await innerAgent.RunAsync(messages, memoryThread, options, cancellationToken);

        var threadState = await innerAgent.SerializeSessionAsync(memoryThread, cancellationToken: cancellationToken);

        await repository.SaveAsync(innerAgent.Name!, threadId.ToString(), threadState.GetRawText());

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
        if (options == null)
        {
            logger.LogError("AgentRunOptions is null. AG-UI middleware requires an Agent Run Options.");
            throw new ArgumentException("AgentRunOptions is null");
        }

        var threadId = options.GetThreadId();

        var memoryThread = await LoadAsync(innerAgent, threadId);

        await foreach (var update in innerAgent.RunStreamingAsync(messages, memoryThread, options, cancellationToken))
        {
            yield return update;
        }

        var threadState = await innerAgent.SerializeSessionAsync(memoryThread, cancellationToken: cancellationToken);

        await repository.SaveAsync(innerAgent.Name!, threadId.ToString(), threadState.GetRawText());
    }

    private async Task<AgentSession> LoadAsync(AIAgent agent, Guid threadId)
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
    }
}

public interface IAgentThreadMiddleware
{
    IAsyncEnumerable<AgentResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken);

    Task<AgentResponse> RunAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken);
}

public interface IAgentMiddleware
{
    IAsyncEnumerable<AgentResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken);

    Task<AgentResponse> RunAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken);

    public string Name { get; }
}