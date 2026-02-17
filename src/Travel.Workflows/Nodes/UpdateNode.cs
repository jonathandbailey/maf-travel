using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class UpdateNode() : Executor<TravelPlanUpdateCommand, TravelPlanContextUpdated>(NodeNames.UpdateNode)
{
    public override async ValueTask<TravelPlanContextUpdated> HandleAsync(TravelPlanUpdateCommand command, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.UpdateNode, Guid.NewGuid());

        var travelPlan = await context.GetTravelPlan(cancellationToken);

        activity?.AddTravelPlanStateSnapshotBefore(travelPlan);

        travelPlan.ApplyPatch(command.TravelPlan);

        activity?.AddTravelPlanStateSnapshotAfter(travelPlan);

        await context.SetTravelPlan(travelPlan, cancellationToken);

        await context.AddEventAsync(new TravelPlanUpdateEvent(travelPlan), cancellationToken);

        return new TravelPlanContextUpdated();
    }
}