using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Travel.Workflows.Nodes;

public class StartNode() : ReflectingExecutor<StartNode>("Start"), IMessageHandler<FunctionCallContent>
{
    public ValueTask HandleAsync(FunctionCallContent message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        return new ValueTask();
    }
}