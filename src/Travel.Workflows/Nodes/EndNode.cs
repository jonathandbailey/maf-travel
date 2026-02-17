using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class EndNode() : Executor<TravelPlanCompletedCommand>(NodeNames.EndNode)
{
    public override async ValueTask HandleAsync(TravelPlanCompletedCommand message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.EndNode, Guid.NewGuid());


        var travelPlan = await context.GetTravelPlan(cancellationToken);

        await context.AddEventAsync(new TravelPlanningCompleteEvent(travelPlan), cancellationToken);
    }
}