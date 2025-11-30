using Application.Infrastructure;
using Application.Observability;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Diagnostics;

namespace Application.Agents;

public class Agent(AIAgent agent, IAgentThreadRepository repository, AgentTypes type) : IAgent
{
    private Activity? _activity;
    
    public async Task<AgentRunResponse> RunAsync(
        IEnumerable<ChatMessage> messages,
        Guid sessionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        Trace();

        var thread = await LoadAsync(userId, sessionId, type);

        var response =  await agent.RunAsync(messages, thread, cancellationToken: cancellationToken);

        Trace(response);
        
        var threadState = thread.Serialize();

        await repository.SaveAsync(userId, sessionId, new AgentState(threadState), type.ToString());

        TraceEnd();

        return response;
    }

    public async Task<AgentRunResponse> RunAsync(
        ChatMessage message,
        Guid sessionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        Trace();

        var thread = await LoadAsync(userId, sessionId, type);

        var response = await agent.RunAsync(message, thread, cancellationToken: cancellationToken);

        Trace(response);

        var threadState = thread.Serialize();

        await repository.SaveAsync(userId, sessionId, new AgentState(threadState), type.ToString());

        TraceEnd();

        return response;
    }

    private void Trace()
    {
        _activity = Telemetry.Start("Agent");
    }

    private void TraceEnd()
    {
        _activity?.Dispose();
    }

    private void Trace(AgentRunResponse response)
    {
        _activity?.SetTag("llm.input_tokens", response.Usage?.InputTokenCount ?? 0);
        _activity?.SetTag("llm.output_tokens", response.Usage?.OutputTokenCount ?? 0);
        _activity?.SetTag("llm.total_tokens", response.Usage?.TotalTokenCount ?? 0);
    }

    private async Task<AgentThread> LoadAsync(Guid userId, Guid sessionId, AgentTypes agentType)
    {
        AgentThread? thread;

        if (!await repository.ExistsAsync(userId, sessionId, agentType.ToString()))
        {
            thread = agent.GetNewThread();
        }
        else
        {
            var stateDto = await repository.LoadAsync(userId, sessionId, agentType.ToString());

            thread = agent.DeserializeThread(stateDto.Thread);
        }

        return thread;
    }
}
public interface IAgent
{
    Task<AgentRunResponse> RunAsync(IEnumerable<ChatMessage> messages, Guid sessionId, Guid userId, CancellationToken cancellationToken = default);

    Task<AgentRunResponse> RunAsync(
        ChatMessage message,
        Guid sessionId,
        Guid userId,
        CancellationToken cancellationToken = default);
}