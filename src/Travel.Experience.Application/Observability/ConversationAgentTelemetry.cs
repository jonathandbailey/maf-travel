using System.Diagnostics;
using A2A;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace Travel.Experience.Application.Observability;

public static class ConversationAgentTelemetry
{
    private const string Name = "Conversation";
    
    private static readonly ActivitySource Source = new ActivitySource("Travel.Experience.Agent", "1.0.0");

    public static Activity? Start(string input, string threadId)
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
}