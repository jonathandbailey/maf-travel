using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public partial class ExtractionNode(AIAgent agent) : Executor<ChatMessage>(NodeNames.ExtractionNodeName)
{
    [MessageHandler]
    public override async ValueTask HandleAsync(ChatMessage message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.ExtractionNodeName, Guid.NewGuid());

        activity?.AddNodeAgentInput(message.Text);

        var response = await agent.RunAsync(message,  cancellationToken: cancellationToken);

        activity?.AddNodeAgentOutput(response.Text);
        activity?.AddNodeAgentUsage(response);

        response.TraceToolCalls(activity);

        if (response.TryGetFunctionArgument<TravelPlanDto>(WorkflowConstants.ExtractingNodeUpdatePlanFunctionName, out var details, Json.FunctionCallSerializerOptions))
        {
            await context.SendMessageAsync(new TravelPlanUpdateCommand(details), cancellationToken);
        }
    }
}