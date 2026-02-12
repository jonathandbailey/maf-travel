using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;

namespace Travel.Workflows.Nodes;

public class TravelPlanNode() : ReflectingExecutor<TravelPlanNode>("TravelPlan"),
    IMessageHandler<TravelPlanUpdateCommand, TravelPlanContextUpdated>

{
    public async ValueTask<TravelPlanContextUpdated> HandleAsync(TravelPlanUpdateCommand command, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        await context.QueueStateUpdateAsync("TravelPlan", command.TravelPlan, scopeName: "TravelPlanScope", cancellationToken);

        await context.AddEventAsync(new TravelPlanUpdateEvent(command.TravelPlan), cancellationToken);

        return new TravelPlanContextUpdated();
    }
}