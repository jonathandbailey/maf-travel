using System.Diagnostics;
using System.Text.Json;
using A2A;
using Agents.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace Travel.Experience.Application.Observability;

public static class ConversationAgentTelemetry
{
    private const string Name = "Conversation";
    
    private static readonly ActivitySource Source = new ActivitySource("Travel.Experience.Agent", "1.0.0");

    public static Activity? Start(string input, Guid threadId)
    {
        var tags = new ActivityTagsCollection
        {
            { "gen_ai.agent.name", Name },
            { "gen_ai.prompt", input },
            { "gen_ai.conversation.id", threadId }
        };

        var source = Source.StartActivity($"invoke_agent {Name}", ActivityKind.Internal, null, tags);
     
        return source;
    }

    public static Activity? StartTool(string key, IDictionary<string,object?> arguments, Activity? parent)
    {
        var tags = new ActivityTagsCollection
        {
            { "gen_ai.tool.name", key },
            { "gen_ai.tool.parameters", arguments }
        };

        var source = Source.StartActivity($"execute_tool {key}", ActivityKind.Internal, parent?.Id, tags);

        return source;
    }

    public static Activity? StartTool(string key, FunctionCallContent functionCallContent, Activity? parent)
    {
        var arguments = "No Arguments in Function Call";

        if (functionCallContent.Arguments != null)
        {
            arguments = JsonSerializer.Serialize(functionCallContent.Arguments);
        }

        var tags = new ActivityTagsCollection
        {
            { "gen_ai.tool.name", key },
            { "gen_ai.tool.parameters", arguments }
        };

        var source = Source.StartActivity($"execute_tool {key}", ActivityKind.Internal, parent?.Id, tags);

        return source;
    }

    public static Activity? StartTool(string key, string arguments, Activity? parent)
    {
        var tags = new ActivityTagsCollection
        {
            { "gen_ai.tool.name", key },
            { "gen_ai.tool.parameters", arguments }
        };

        var source = Source.StartActivity($"execute_tool {key}", ActivityKind.Internal, parent?.Id, tags);

        return source;
    }

    public static Activity? AddEvent(this Activity activity, AgentResponseUpdate agentRunUpdate, TaskState state, string content)
    {
        var updateType = agentRunUpdate.RawRepresentation?.GetType().Name ?? "Unknown";


        activity?.AddEvent(new ActivityEvent("gen_ai.stream.chunk",
            tags: new ActivityTagsCollection
            {
                { "gen_ai.content.part", content },
                { "gen_ai.task.state", state.ToString()},
                { "gen_ai.event.type", updateType },
            }));

        return activity;
    }

    public static Activity? AddEvent(this Activity activity, WorkflowEvent workflowEvent, string content)
    {
        var updateType = workflowEvent.GetType().Name ?? "Unknown";


        activity?.AddEvent(new ActivityEvent("gen_ai.stream.chunk",
            tags: new ActivityTagsCollection
            {
                { "gen_ai.content.part", content },

                { "gen_ai.event.type", updateType },
            }));

        return activity;
    }

    public static Activity? AddEvent(this Activity activity, AgentResponseUpdate agentRunUpdate, string content)
    {
        var updateType = agentRunUpdate.RawRepresentation?.GetType().Name ?? "Unknown";


        activity?.AddEvent(new ActivityEvent("gen_ai.stream.chunk",
            tags: new ActivityTagsCollection
            {
                { "gen_ai.content.part", content },

                { "gen_ai.event.type", updateType },
            }));

        return activity;
    }

    public static void SetToolCallCount(this Activity? activity, int count) =>
        activity?.SetTag("gen_ai.tool.call_count", count);

    public static void SetToolResultCount(this Activity? activity, int count) =>
        activity?.SetTag("gen_ai.tool.result_count", count);

    public static void RecordToolResponseMessage(this Activity? activity, ChatMessage message) =>
        activity?.AddEvent(new ActivityEvent("tool.response_message",
            tags: new ActivityTagsCollection
            {
                { "gen_ai.message.role", message.Role.Value },
                { "gen_ai.message.content_count", message.Contents.Count },
            }));

    public static void SetError(this Activity? activity, Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.SetTag("error.type", ex.GetType().Name);
    }

    public static void AddEvent(this Activity? activity, ToolStatusUpdate update) =>
        activity?.AddEvent(new ActivityEvent("tool.status",
            tags: new ActivityTagsCollection
            {
                { "tool.status.message", update.Message },
                { "tool.status.thought", update.Thought },
                { "tool.status.source", update.Source },
            }));

    public static void AddEvent(this Activity? activity, ToolStateSnapshotUpdate update) =>
        activity?.AddEvent(new ActivityEvent("tool.state_snapshot",
            tags: new ActivityTagsCollection
            {
                { "tool.snapshot.type", update.Type },
                { "tool.snapshot.data", JsonSerializer.Serialize(update.Data) },
            }));

    public static void AddEvent(this Activity? activity, ToolResultUpdate update) =>
        activity?.AddEvent(new ActivityEvent("tool.result",
            tags: new ActivityTagsCollection
            {
                { "tool.result.call_id", update.Result.CallId },
            }));

    public static void AddEvent(this Activity? activity, ToolErrorUpdate update) =>
        activity?.AddEvent(new ActivityEvent("tool.error",
            tags: new ActivityTagsCollection
            {
                { "tool.error.message", update.Message },
            }));

    public static void SetCancelled(this Activity? activity, string operationName)
    {
        activity?.SetStatus(ActivityStatusCode.Error, $"{operationName} was cancelled");
        activity?.SetTag("cancellation.requested", true);
    }

    public static void SetIncomplete(this Activity? activity, string operationName) =>
        activity?.SetStatus(ActivityStatusCode.Error, $"{operationName} did not complete");

    public static void SetError(this Activity? activity, string message) =>
        activity?.SetStatus(ActivityStatusCode.Error, message);
}