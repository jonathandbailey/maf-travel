using Agents.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Agents.Middleware;

public class AgentAgUiMiddleware(ILogger<IAgentAgUiMiddleware> logger) : IAgentAgUiMiddleware
{
    public async IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (options == null)
        {
            logger.LogError("AgentRunOptions is null. AG-UI middleware requires an Agent Run Options.");
            throw new ArgumentException("AgentRunOptions is null");
        }
        
        var threadId = options.GetAgUiThreadId();
       
        options = options.AddThreadId(threadId);

        await foreach (var update in innerAgent.RunStreamingAsync(messages, thread, options, cancellationToken))
        {
            yield return update;
        }
    }
}

public interface IAgentAgUiMiddleware
{
    IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken);
}