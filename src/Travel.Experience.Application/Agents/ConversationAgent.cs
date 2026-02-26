using Agents;
using Agents.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using Travel.Experience.Application.Agents.ToolHandling;
using Travel.Experience.Application.Extensions;
using Travel.Experience.Application.Observability;

namespace Travel.Experience.Application.Agents;

public class ConversationAgent(AIAgent agent, IConversationToolHandlerRegistry registry) : DelegatingAIAgent(agent)
{
    private const string StatusMessageThinking = "Thinking...";
    private const string ProcessingResults = "Processing Results...";

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
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

        if (tools.Count == 0)
            yield break;

        var toolResults = new List<AIContent>();

        foreach (var (toolName, call) in tools)
        {
            using var toolActivity = ConversationAgentTelemetry.StartTool(
                toolName, call.Arguments?.ToString() ?? string.Empty, agentActivity);

            var handler = registry.GetHandler(toolName);
            if (handler is null) continue;

            await foreach (var update in handler.ExecuteAsync(
                call, new ToolHandlerContext(threadId), cancellationToken))
            {
                if (update is ToolStatusUpdate statusUpdate)
                    yield return statusUpdate.Message.ToAgentResponseStatusMessage();
                else if (update is ToolResultUpdate resultUpdate)
                    toolResults.Add(resultUpdate.Result);
            }
        }

        if (toolResults.Count == 0)
            yield break;

        yield return ProcessingResults.ToAgentResponseStatusMessage();

        await foreach (var update in InnerAgent.RunStreamingAsync(
            [new ChatMessage(ChatRole.Tool, toolResults)], thread,
            cancellationToken: cancellationToken))
        {
            yield return update;
        }
    }
}
