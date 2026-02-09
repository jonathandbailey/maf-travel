using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Travel.Workflows.Planning.Nodes;

public class FinalizerNode() : ReflectingExecutor<FinalizerNode>("Finalizer"), IMessageHandler<FunctionCallContent>
{
    public ValueTask HandleAsync(FunctionCallContent message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return new ValueTask();
    }
}