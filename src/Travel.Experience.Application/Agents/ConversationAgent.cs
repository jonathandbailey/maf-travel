using Agents;
using Agents.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using Agents.Tools;
using Travel.Experience.Application.Extensions;
using Travel.Experience.Application.Observability;

namespace Travel.Experience.Application.Agents;

public class ConversationAgent(AIAgent agent, IToolRegistry registry) : DelegatingAIAgent(agent)
{
    private const string StatusMessageThinking = "Thinking...";
   
    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? thread = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var localMessages = messages.ToList();

        Verify.NotNull(options);
        
        Verify.NotEmpty(localMessages);

        var threadId = options.GetAgUiThreadId();
        options = options.AddThreadId(threadId);

        using var agentActivity = ConversationAgentTelemetry.Start(localMessages.First().Text, threadId);
      
        var tools = new Dictionary<string, FunctionCallContent>();

        var localThread = await InnerAgent.CreateSessionAsync(cancellationToken);

        var response = await InnerAgent.RunAsync(localMessages, localThread, options, cancellationToken);

        foreach (var responseMessage in response.Messages)
        {
            tools.AddToolCalls(responseMessage.Contents);

            if (responseMessage.Role == ChatRole.Assistant)
            {
                yield return StatusMessageThinking.ToAgentResponseStatusMessage(thought:responseMessage.Text);
            }
        }

        agentActivity.SetToolCallCount(tools.Count);

        if (tools.Count == 0)
            yield break;

        var toolResults = new List<AIContent>();

        foreach (var (toolName, call) in tools)
        {
            using var toolActivity = ConversationAgentTelemetry.StartTool(toolName, call, agentActivity);

            var handler = registry.GetHandler(toolName);
            if (handler is null) continue;

            await foreach (var update in handler.ExecuteAsync(
                call, new ToolHandlerContext(threadId), cancellationToken))
            {
                switch (update)
                {
                    case ToolStatusUpdate statusUpdate:
                        toolActivity.AddEvent(statusUpdate);
                        yield return statusUpdate.Message.ToAgentResponseStatusMessage(statusUpdate.Thought, statusUpdate.Source);
                        break;
                    case ToolStateSnapshotUpdate snapshotUpdate:
                        toolActivity.AddEvent(snapshotUpdate);
                        yield return snapshotUpdate.Data.ToAgentResponseStateSnapshot(snapshotUpdate.Type);
                        break;
                    case ToolResultUpdate resultUpdate:
                        toolActivity.AddEvent(resultUpdate);
                        toolResults.Add(resultUpdate.Result);
                        break;
                }
            }
        }

        if (toolResults.Count == 0)
            yield break;
   
        var message = new ChatMessage(ChatRole.Tool, toolResults);

        await foreach (var update in InnerAgent.RunStreamingAsync([message], localThread, options, cancellationToken))
        {
            yield return update;
        }
    }
}
