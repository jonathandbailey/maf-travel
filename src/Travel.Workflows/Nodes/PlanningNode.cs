using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Travel.Workflows.Nodes;

public class PlanningNode(AIAgent agent) : ReflectingExecutor<PlanningNode>("Planning"), IMessageHandler<ChatMessage, AgentResponse>
{
    public async ValueTask<AgentResponse> HandleAsync(ChatMessage message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var response = await agent.RunAsync(message, cancellationToken: cancellationToken);

        return response;
    }
}