using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Common;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;
using Travel.Workflows.TravelPlanCriteria.Dto;

namespace Travel.Workflows.TravelPlanCriteria.Nodes;

public partial class InformationResponseNode() : Executor(NodeNames.InformationResponseNode)
{
    [MessageHandler(Send = [typeof(TravelPlanExtractCommand)])]
    private async ValueTask HandleAsync(InformationResponse informationResponse, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var threadId = await context.GetThreadId(cancellationToken);

        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.InformationResponseNode, threadId);

        await context.SendMessageAsync(new TravelPlanExtractCommand(informationResponse.Message), cancellationToken);
    }
}