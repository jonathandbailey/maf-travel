using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Common;
using Travel.Workflows.Common.Extensions;
using Travel.Workflows.Common.Telemetry;
using Travel.Workflows.TravelPlanCriteria.Dto;

namespace Travel.Workflows.TravelPlanCriteria.Nodes;

public partial class InformationRequestNode() : Executor(NodeNames.InformationRequestNode)
{
    [MessageHandler(Send = [typeof(InformationRequest)])]
    private async ValueTask HandleAsync(RequestInformationCommand command, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var threadId = await context.GetThreadId(cancellationToken);

        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.InformationRequestNode, threadId);

        await context.SendMessageAsync(new InformationRequest(command.Details.Message, command.Details.RequiredInputs),
            cancellationToken: cancellationToken);
    }
}