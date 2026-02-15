using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class TravelPlanUpdateNode() : ReflectingExecutor<TravelPlanUpdateNode>("TravelPlanUpdate"),
    IMessageHandler<TravelPlanUpdateCommand, TravelPlanContextUpdated>

{
    public async ValueTask<TravelPlanContextUpdated> HandleAsync(TravelPlanUpdateCommand command, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode("TravelPlanUpdate", Guid.NewGuid());


        var travelPlan = await context.GetTravelPlan(cancellationToken);

        travelPlan.ApplyPatch(command.TravelPlan);

        await context.SetTravelPlan(travelPlan, cancellationToken);

        await context.AddEventAsync(new TravelPlanUpdateEvent(travelPlan), cancellationToken);

        return new TravelPlanContextUpdated();
    }
}