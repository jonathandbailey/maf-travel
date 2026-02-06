using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Travel.Workflows.Planning.Nodes;

public class TravelPlanNode() : ReflectingExecutor<TravelPlanNode>("TravelPlan"), IMessageHandler<FunctionCallContent, ChatMessage>
{
    public ValueTask<ChatMessage> HandleAsync(FunctionCallContent message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        return new ValueTask<ChatMessage>(new ChatMessage(ChatRole.User, "Travel plan updated successfully."));
    }
}