using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Dto;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;


public partial class ExtractionNode(AIAgent agent) : Executor(NodeNames.ExtractionNodeName)
{

    [MessageHandler(Send = [typeof(TravelPlanUpdateCommand)])]
    private async ValueTask HandleAsync(TravelPlanExtractCommand command, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.ExtractionNodeName, Guid.NewGuid());

        activity?.AddNodeAgentInput(command.Message.Text);

        var response = await agent.RunAsync(command.Message,  cancellationToken: cancellationToken);

        activity?.AddNodeAgentOutput(response.Text);
        activity?.AddNodeAgentUsage(response);

        response.TraceToolCalls(activity);

        if (response.TryGetFunctionArgument<TravelPlanDto>(WorkflowConstants.ExtractingNodeUpdatePlanFunctionName, out var details, Json.FunctionCallSerializerOptions))
        {
            await context.SendMessageAsync(new TravelPlanUpdateCommand(details), cancellationToken);
        }
    }
}