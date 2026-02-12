using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;

namespace Travel.Workflows.Nodes;

public class EndNode() : ReflectingExecutor<StartNode>("End"), IMessageHandler<FunctionCallContent>
{
    public async ValueTask HandleAsync(FunctionCallContent message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var travelPlan = await context.ReadStateAsync<TravelPlanDto>("TravelPlan", scopeName: "TravelPlanScope", cancellationToken: cancellationToken)
                         ?? throw new InvalidOperationException("File content state not found");

        await context.AddEventAsync(new TravelPlanningCompleteEvent(travelPlan), cancellationToken);
    }
}