using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Flights.Dto;

namespace Travel.Workflows.Flights.Nodes;

public partial class StartNode() : Executor(FlightsNodeNames.StartNode)
{
    [MessageHandler(Send = [typeof(FlightSearchAgentCommand)])]
    private async ValueTask HandleAsync(FlightsWorkflowRequest request, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        await context.SendMessageAsync(
            new FlightSearchAgentCommand(
                request.Origin,
                request.Destination,
                request.DepartureDate,
                request.ReturnDate,
                request.Passengers),
            cancellationToken);
    }
}
