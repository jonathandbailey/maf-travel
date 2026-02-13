using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Extensions;

namespace Travel.Workflows.Nodes;

public class EndNode() : ReflectingExecutor<EndNode>("End"), IMessageHandler<TravelPlanCompletedCommand>
{
    public async ValueTask HandleAsync(TravelPlanCompletedCommand message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var travelPlan = await context.GetTravelPlan(cancellationToken);

        await context.AddEventAsync(new TravelPlanningCompleteEvent(travelPlan), cancellationToken);
    }
}