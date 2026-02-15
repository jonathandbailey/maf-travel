using Microsoft.Agents.AI;
using Microsoft.Agents.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

namespace Travel.Workflows.Telemetry;

public static class TravelWorkflowTelemetry
{
    private static readonly ActivitySource Source = new ActivitySource("Travel.Workflows", "1.0.0");
    private static readonly int MaxPreviewLength = 100;

    public static Activity? Start(string name)
    {
        return Source.StartActivity(name);
    }

    public static Activity? ToolCall(string key, object? arguments, Activity? parent)
    {
        var tags = new ActivityTagsCollection
        {
            { "gen_ai.tool.name", key },
        };

        var jsonArgs = JsonSerializer.Serialize(arguments);

        var inputEvent = new ActivityEvent("ToolInput", tags: new ActivityTagsCollection
        {
            { "arguments", jsonArgs }
        });
        

        var source = Source.StartActivity($"execute_tool {key}", ActivityKind.Internal, parent?.Id, tags);

        source?.AddEvent(inputEvent);

        return source;
    }

    public static Activity? InvokeNode(string name, Guid threadId)
    {
        var tags = new ActivityTagsCollection
        {
            { "gen_ai.workflow.name", name },
            { "gen_ai.conversation.id", threadId }
        };

        return Source.StartActivity($"invoke_node {name}", ActivityKind.Internal, null, tags);
    }

    public static void AddNodeAgentUsage(this Activity activity, AgentResponse agentWorkflowResponse)
    {
        activity.SetTag("gen_ai.usage.input_tokens", agentWorkflowResponse.Usage?.InputTokenCount );
        activity.SetTag("gen_ai.usage.output_tokens", agentWorkflowResponse.Usage?.OutputTokenCount );
        activity.SetTag("gen_ai.usage.total_tokens", agentWorkflowResponse.Usage?.TotalTokenCount );
    }

    public static void AddNodeAgentInput(this Activity activity, string input)
    {
        activity.SetTag("gen_ai.task.input", Truncate(input));
    }

    public static void AddNodeAgentOutput(this Activity activity, string output)
    {
        activity.SetTag("gen_ai.task.output", Truncate(output));
    }

    private static string Truncate(string value)
    {
        var truncated = value.Length > MaxPreviewLength;

        var preview = truncated ? value[..MaxPreviewLength] : value;

        if (truncated)
        {
            preview += " -[Truncated]";
        }

        return preview;

    }

}