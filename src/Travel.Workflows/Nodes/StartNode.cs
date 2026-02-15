using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Travel.Workflows.Dto;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class StartNode() : ReflectingExecutor<StartNode>("Start"), IMessageHandler<ChatMessage, ChatMessage>
{
    public async ValueTask<ChatMessage> HandleAsync(ChatMessage message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode("StartNode", Guid.NewGuid());
        
        await context.SetTravelPlan(new TravelPlanDto(), cancellationToken);

        return message;
    }
}