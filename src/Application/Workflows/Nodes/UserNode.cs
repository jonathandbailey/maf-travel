using System.Text;
using Application.Agents;
using Application.Observability;
using Application.Workflows.Dto;
using Application.Workflows.Events;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Nodes;

public class UserNode(IAgent agent) : ReflectingExecutor<UserNode>(WorkflowConstants.UserNodeName), 
    IMessageHandler<RequestUserInput>
{
    public async ValueTask HandleAsync(RequestUserInput requestUserInput, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.UserNodeName}{WorkflowConstants.HandleRequest}");

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.InputNodePreview, requestUserInput.Message);

        var stringBuilder = new StringBuilder();

        await foreach (var update in agent.RunStreamingAsync(new ChatMessage(ChatRole.User, requestUserInput.Message), cancellationToken: cancellationToken))
        {
            await context.AddEventAsync(new UserStreamingEvent(update.Text), cancellationToken);

            stringBuilder.Append(update.Text);
        }

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.OutputNodePreview, stringBuilder.ToString());

        await context.AddEventAsync(new UserStreamingCompleteEvent(), cancellationToken);

        await context.SendMessageAsync(new UserRequest(stringBuilder.ToString()), cancellationToken: cancellationToken);
    }
}