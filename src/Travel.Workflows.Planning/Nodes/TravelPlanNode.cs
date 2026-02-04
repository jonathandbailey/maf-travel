using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Travel.Workflows.Planning.Nodes;

public class TravelPlanNode() : ReflectingExecutor<TravelPlanNode>("TravelPlan"), IMessageHandler<FunctionCallContent>
{
    public async ValueTask HandleAsync(FunctionCallContent message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}