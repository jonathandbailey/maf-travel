using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Travel.Workflows.Planning.Nodes;

public class RequestInformationNode() : ReflectingExecutor<RequestInformationNode>("RequestInformation"), IMessageHandler<FunctionCallContent>
{
    public async ValueTask HandleAsync(FunctionCallContent message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}