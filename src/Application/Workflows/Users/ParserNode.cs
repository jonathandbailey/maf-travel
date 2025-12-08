using Application.Agents;
using Application.Observability;
using Application.Workflows.ReAct.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Users;

public class ParserNode(IAgent agent) : ReflectingExecutor<ParserNode>(WorkflowConstants.ParserNodeName), IMessageHandler<UserInput, ActObservation>
{
    public async ValueTask<ActObservation> HandleAsync(UserInput message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.ParserNodeName}.observe");

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.InputNodePreview, message.Message);

        var response = await agent.RunAsync(new ChatMessage(ChatRole.User, message.Message), cancellationToken);

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.OutputNodePreview, response.Text);


        return new ActObservation(response.Text, "UserInput");
    }
}