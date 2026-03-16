using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Flights.Dto;

namespace Travel.Workflows.Flights.Nodes;

public partial class StartNode() : Executor(FlightsNodeNames.StartNode)
{
    [MessageHandler]
    private async ValueTask HandleAsync(FlightsWorkflowRequest request, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {

    }
}