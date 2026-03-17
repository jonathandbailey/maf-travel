using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Common.Telemetry;
using Travel.Workflows.Flights.Dto;
using Travel.Workflows.Flights.Events;

namespace Travel.Workflows.Flights.Nodes;

public partial class FlightsEndNode() : Executor(FlightsNodeNames.EndNode)
{
    [MessageHandler]
    private async ValueTask HandleAsync(FlightSearchResult result, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(FlightsNodeNames.EndNode, Guid.Empty);

        await context.AddEventAsync(new FlightSearchCompleteEvent(result.Flights), cancellationToken);
    }
}
