using A2A;
using Agents.Extensions;
using Agents.Observability;
using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Agents;

public class UserAgent(AIAgent agent, IA2AAgentServiceDiscovery discovery) : DelegatingAIAgent(agent)
{
    protected override async IAsyncEnumerable<AgentRunResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages,
        AgentThread? thread = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"UserAgent.Run");
        activity?.SetTag("Input", messages.First().Text);


        var threadId = options?.GetAgUiThreadId();

        options = options.AddThreadId(threadId!);

        var stateBytes = JsonSerializer.SerializeToUtf8Bytes("Thinking...");

        yield return new AgentRunResponseUpdate
        {
            Contents = [new DataContent(stateBytes, "application/json")]
        };

        var tools = new Dictionary<string, FunctionCallContent>();

        await foreach (var update in InnerAgent.RunStreamingAsync(messages, thread, options, cancellationToken))
        {
            foreach (var content in update.Contents)
            {
                if (content is FunctionCallContent callContent)
                {
                    tools[callContent.Name] = callContent;
                }
            }

            yield return update;
        }

        var toolResults = new List<AIContent>();

        stateBytes = JsonSerializer.SerializeToUtf8Bytes("Executing Travel Workflow...");

        yield return new AgentRunResponseUpdate
        {
            Contents = [new DataContent(stateBytes, "application/json")]
        };

        foreach (var functionCallContent in tools)
        {
            var agentMeta = discovery.GetAgentMeta(functionCallContent.Key);

            var agentThread = agentMeta.Agent.GetNewThread(threadId!);

            var argument = functionCallContent.Value.Arguments["jsonPayload"].ToString();


            try
            {
                await foreach (var agentRunUpdate in agentMeta.Agent.RunStreamingAsync(new ChatMessage(ChatRole.User, argument), agentThread, cancellationToken: cancellationToken))
                {
                    if (agentRunUpdate.RawRepresentation is TaskArtifactUpdateEvent)
                    {
                        var artifactEvent = agentRunUpdate.RawRepresentation as TaskArtifactUpdateEvent;
                        var messageText = artifactEvent.Artifact.Parts.OfType<TextPart>().First().Text;

                        toolResults.Add(new FunctionResultContent(
                            functionCallContent.Value.CallId,
                            messageText));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        stateBytes = JsonSerializer.SerializeToUtf8Bytes("Processing Results...");

        yield return new AgentRunResponseUpdate
        {
            Contents = [new DataContent(stateBytes, "application/json")]
        };

        await foreach (var update in InnerAgent.RunStreamingAsync([new ChatMessage(ChatRole.Tool, toolResults)], thread, cancellationToken: cancellationToken))
        {
            yield return update;
        }
    }
}