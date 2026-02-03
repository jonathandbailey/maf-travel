using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Travel.Workflows.Planning.Nodes;

public class PlanningNode(AIAgent agent) : ReflectingExecutor<PlanningNode>("Planning"), IMessageHandler<ChatMessage, List<FunctionCallContent>>
{
    public async ValueTask<List<FunctionCallContent>> HandleAsync(ChatMessage message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var response = await agent.RunAsync(message, cancellationToken: cancellationToken);

        var toolCalls = new List<FunctionCallContent>();

        foreach (var msg in response.Messages)
        {
            foreach (var content in msg.Contents)
            {
                if (content is FunctionCallContent functionCall)
                {
                    toolCalls.Add(functionCall);
                }
            }
        }

        return toolCalls;
    }
}