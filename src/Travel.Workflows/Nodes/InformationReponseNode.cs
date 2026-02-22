using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Workflows.Dto;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public partial class InformationResponseNode() : Executor(NodeNames.InformationResponseNode)
{
    [MessageHandler(Send = [typeof(ChatMessage)])]
    private async ValueTask HandleAsync(InformationResponse informationResponse, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.InformationResponseNode, Guid.NewGuid());

        await context.SendMessageAsync(informationResponse.Message, cancellationToken);
    }
}