using System.Runtime.CompilerServices;
using Agents;
using Agents.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Travel.Experience.Api.Extensions;
using Travel.Experience.Api.Observability;

namespace Travel.Experience.Api.Agents;

public class ConversationAgent(AIAgent agent) : DelegatingAIAgent(agent)
{
    private const string StatusMessageThinking = "Thinking...";
    private const string ExecutingTravelWorkflow = "Executing Travel Workflow...";
    private const string ProcessingResults = "Processing Results...";
    
    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages,
        AgentSession? thread = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var localMessages = messages.ToList();

        Verify.NotNull(options);
        Verify.NotNull(thread);
        Verify.NotEmpty(localMessages);
      
        var threadId = options.GetAgUiThreadId();

        options = options.AddThreadId(threadId);

        using var agentActivity = ConversationAgentTelemetry.Start(localMessages.First().Text, threadId);

        yield return StatusMessageThinking.ToAgentResponseStatusMessage();
 
        await foreach (var update in InnerAgent.RunStreamingAsync(localMessages, thread, options, cancellationToken))
        {
            yield return update;
        }
        
     
        yield return ExecutingTravelWorkflow.ToAgentResponseStatusMessage();


      
   
        yield return ProcessingResults.ToAgentResponseStatusMessage();

       
    }
}