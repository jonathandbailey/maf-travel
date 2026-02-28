using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Agents.Middleware;

public interface IAgentAgUiMiddleware
{
    IAsyncEnumerable<AgentResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken);
}
