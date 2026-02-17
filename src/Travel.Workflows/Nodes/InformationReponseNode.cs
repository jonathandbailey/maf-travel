using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Dto;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class InformationResponseNode() : Executor<InformationResponse>(NodeNames.InformationResponseNode)
{
    public override async ValueTask HandleAsync(InformationResponse informationResponse, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.InformationResponseNode, Guid.NewGuid());

        await context.SendMessageAsync(informationResponse.Message, cancellationToken);
    }
}