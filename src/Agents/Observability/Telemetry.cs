using System.Diagnostics;
using A2A;
using Microsoft.Agents.AI;

namespace Agents.Observability;

public static class UserAgentTelemetry
{
    private const string Name = "User";
    
    private static readonly ActivitySource Source = new ActivitySource("Travel.Experience.Agents", "1.0.0");

    public static Activity? Start(string input, string threadId)
    {
        var tags = new ActivityTagsCollection
        {
            { "StartInput", input },
            { "ThreadId", threadId }
        };

        var source = Source.StartActivity(Name, ActivityKind.Client, null, tags);
     
        return source;
    }

    public static Activity? StartTool(string key, string arguments)
    {
        var tags = new ActivityTagsCollection
        {
            { "Key", key },
            { "Arguments", arguments }
        };

        var source = Source.StartActivity($"{Name}.ToolCall", ActivityKind.Client, null, tags);

        return source;
    }

    public static Activity? AddEvent(this Activity activity, AgentRunResponseUpdate agentRunUpdate, TaskState state, string content)
    {
        var updateType = agentRunUpdate.RawRepresentation?.GetType().Name ?? "Unknown";


        activity?.AddEvent(new ActivityEvent(updateType,
            tags: new ActivityTagsCollection
            {
                { "Content", content },
                { "TaskState", state.ToString()}
            }));

        return activity;
    }

    public static Activity? AddEvent(this Activity activity, AgentRunResponseUpdate agentRunUpdate, string content)
    {
        var updateType = agentRunUpdate.RawRepresentation?.GetType().Name ?? "Unknown";


        activity?.AddEvent(new ActivityEvent(updateType,
            tags: new ActivityTagsCollection
            {
                { "Content", content }
            }));

        return activity;
    }
}