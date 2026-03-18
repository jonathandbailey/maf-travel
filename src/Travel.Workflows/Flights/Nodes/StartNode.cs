using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Flights.Dto;

namespace Travel.Workflows.Flights.Nodes;

public partial class StartNode() : Executor(FlightsNodeNames.StartNode)
{
    [MessageHandler(Send = [typeof(FlightSearchAgentCommand)])]
    private async ValueTask HandleAsync(FlightsWorkflowInput input, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        await context.SendMessageAsync(
            new FlightSearchAgentCommand(
                input.Origin,
                input.Destination,
                input.DepartureDate,
                input.ReturnDate,
                input.Passengers),
            cancellationToken);
    }
}
