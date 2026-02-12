using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Travel.Workflows.Events;

namespace Travel.Workflows.Nodes;

public class EndNode() : ReflectingExecutor<StartNode>("End"), IMessageHandler<FunctionCallContent>
{
    public async ValueTask HandleAsync(FunctionCallContent message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        await context.AddEventAsync(new TravelPlanningCompleteEvent(), cancellationToken);
    }
}