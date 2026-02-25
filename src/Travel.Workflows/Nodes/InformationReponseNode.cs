using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public partial class InformationResponseNode() : Executor(NodeNames.InformationResponseNode)
{
    [MessageHandler(Send = [typeof(TravelPlanExtractCommand)])]
    private async ValueTask HandleAsync(InformationResponse informationResponse, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.InformationResponseNode, Guid.NewGuid());

        await context.SendMessageAsync(new TravelPlanExtractCommand(informationResponse.Message), cancellationToken);
    }
}