using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Travel.Workflows.Nodes;

public class ExtractingNode(AIAgent agent) : ReflectingExecutor<ExtractingNode>("Extracting"), IMessageHandler<ChatMessage>
{
    public async ValueTask HandleAsync(ChatMessage message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var response = await agent.RunAsync(message,  cancellationToken: cancellationToken);

        foreach (var chatMessage in response.Messages)
        {
            foreach (var content in chatMessage.Contents)
            {
                if (content is FunctionCallContent functionCall)
                {
                    await context.SendMessageAsync(functionCall, cancellationToken: cancellationToken);
                }
            }
        }
    }
}