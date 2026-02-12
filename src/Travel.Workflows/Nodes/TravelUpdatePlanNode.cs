using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Extensions;

namespace Travel.Workflows.Nodes;

public class TravelUpdatePlanNode() : ReflectingExecutor<TravelUpdatePlanNode>("TravelPlanUpdate"),
    IMessageHandler<TravelPlanUpdateCommand, TravelPlanContextUpdated>

{
    public async ValueTask<TravelPlanContextUpdated> HandleAsync(TravelPlanUpdateCommand command, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var travelPlan = await context.GetTravelPlan(cancellationToken);

        var mergedTravelPlan = new TravelPlanDto(
            Origin: command.TravelPlan.Origin ?? travelPlan.Origin,
            Destination: command.TravelPlan.Destination ?? travelPlan.Destination,
            StartDate: command.TravelPlan.StartDate ?? travelPlan.StartDate,
            EndDate: command.TravelPlan.EndDate ?? travelPlan.EndDate,
            NumberOfTravelers: command.TravelPlan.NumberOfTravelers ?? travelPlan.NumberOfTravelers
        );

        await context.SetTravelPlan(mergedTravelPlan, cancellationToken);

        await context.AddEventAsync(new TravelPlanUpdateEvent(mergedTravelPlan), cancellationToken);

        return new TravelPlanContextUpdated();
    }
}