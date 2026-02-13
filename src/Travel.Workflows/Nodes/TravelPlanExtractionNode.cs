using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Extensions;

namespace Travel.Workflows.Nodes;

public class TravelPlanExtractionNode(AIAgent agent) : ReflectingExecutor<TravelPlanExtractionNode>("TravelPlanExtractionNode"), IMessageHandler<ChatMessage>
{
    public async ValueTask HandleAsync(ChatMessage message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var response = await agent.RunAsync(message,  cancellationToken: cancellationToken);

        if (response.TryGetFunctionArgument<TravelPlanDto>(WorkflowConstants.ExtractingNodeUpdatePlanFunctionName, out var details, Json.FunctionCallSerializerOptions))
        {
            await context.SendMessageAsync(new TravelPlanUpdateCommand(details), cancellationToken);
        }
    }
}