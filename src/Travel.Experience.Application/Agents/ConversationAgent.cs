using System.Runtime.CompilerServices;
using Agents;
using Agents.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;
using Travel.Experience.Application.Extensions;
using Travel.Experience.Application.Observability;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Extensions;
using Travel.Workflows.Interfaces;

namespace Travel.Experience.Application.Agents;

public class ConversationAgent(AIAgent agent, IWorkflowFactory workflowFactory) : DelegatingAIAgent(agent)
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


        var tools = new Dictionary<string, FunctionCallContent>();

        await foreach (var update in InnerAgent.RunStreamingAsync(localMessages, thread, options, cancellationToken))
        {
            tools.AddToolCalls(update.Contents);

            yield return update;
        }

        var toolResults = new List<AIContent>();

        foreach (var functionCallContent in tools)
        {
            if (functionCallContent.Key == ConversationAgentTools.RequestInformationToolName)
            {
                var workflow = await workflowFactory.Create();

                if (functionCallContent.Value.TryGetArgument<string>(
                        "request", out var details,
                        Json.FunctionCallSerializerOptions))
                {
                    var request = new TravelWorkflowRequest(new ChatMessage(ChatRole.User, details),
                        Guid.Parse(threadId), new TravelPlanDto());

                    await foreach (var evt in workflow.WatchStreamAsync(request))
                    {
                        if (evt is RequestInfoEvent requestInfoEvent)
                        {

                        }
                    }
                }
            }
        }

        await foreach (var update in InnerAgent.RunStreamingAsync(localMessages, thread, options, cancellationToken))
        {
            yield return update;
        }
        
     
        yield return ExecutingTravelWorkflow.ToAgentResponseStatusMessage();
   
        yield return ProcessingResults.ToAgentResponseStatusMessage();
    }
}