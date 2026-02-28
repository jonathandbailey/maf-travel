using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Agents.Middleware;

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
