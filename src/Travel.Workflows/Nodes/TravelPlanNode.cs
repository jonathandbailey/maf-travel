using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Services;

namespace Travel.Workflows.Nodes;

public class TravelPlanNode(ITravelPlanService travelPlanService) : ReflectingExecutor<TravelPlanNode>("TravelPlan"),
    IMessageHandler<TravelPlanUpdateCommand, TravelPlanContextUpdated>

{
    public async ValueTask<TravelPlanContextUpdated> HandleAsync(TravelPlanUpdateCommand command, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        await travelPlanService.Update(command.TravelPlan);

        await context.QueueStateUpdateAsync("TravelPlan", command.TravelPlan, scopeName: "TravelPlanScope", cancellationToken);

        await context.AddEventAsync(new TravelPlanUpdateEvent(command.TravelPlan), cancellationToken);

        return new TravelPlanContextUpdated();
    }
}