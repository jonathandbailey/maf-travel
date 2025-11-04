using Application.Agents;
using Application.Workflows.Conversations.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Conversations;

public class ActNode(IAgent agent) : ReflectingExecutor<ActNode>("ActNode"), IMessageHandler<ChatMessage, ChatMessage>
{
    public async ValueTask<ChatMessage> HandleAsync(ChatMessage message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var response = await agent.RunAsync(new List<ChatMessage> { message }, cancellationToken: cancellationToken);

        if (JsonOutputParser.HasJson(response.Text))
        {
            var routeAction = JsonOutputParser.Parse<RouteAction>(response.Text);
        }
        
        
        return response.Messages.First();
    }
}