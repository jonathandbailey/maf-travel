using System.Runtime.CompilerServices;
using Agents.Extensions;
using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Agents.Middleware;

public class AgentThreadMiddleware(IAgentMemoryService memory, ILogger<IAgentAgUiMiddleware> logger) :IAgentThreadMiddleware, IAgentMiddleware
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

        var threadState = innerAgent.SerializeSession(memoryThread);

        await memory.SaveAsync(new AgentState(threadState), GetResourceName(innerAgent.Name!, threadId));


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

        var threadState = innerAgent.SerializeSession(memoryThread);

        await memory.SaveAsync(new AgentState(threadState), GetResourceName(innerAgent.Name!, threadId));
    }

    private async Task<AgentSession> LoadAsync(AIAgent agent, Guid threadId)
    {
        AgentSession? thread;

        if (!await memory.ExistsAsync(GetResourceName(agent.Name!, threadId)))
        {
            thread = await agent.CreateSessionAsync();
        }
        else
        {
            var stateDto = await memory.LoadAsync(GetResourceName(agent.Name!, threadId));

            thread = await agent.DeserializeSessionAsync(stateDto.Thread);
        }

        return thread;
    }

    private static string GetResourceName(string agentName, Guid threadId)
    {
        return $"{threadId}/{agentName}";
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