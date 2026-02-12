using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using System.Text.Json;
using Travel.Workflows.Dto;
using Travel.Workflows.Extensions;

namespace Travel.Workflows.Nodes;

public class PlanningNode(AIAgent agent) : ReflectingExecutor<PlanningNode>("Planning"), 
    IMessageHandler<TravelPlanContextUpdated, AgentResponse>
{

    public async ValueTask<AgentResponse> HandleAsync(TravelPlanContextUpdated message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var travelPlan = await context.GetTravelPlan(cancellationToken);

        var serializedPlan = JsonSerializer.Serialize(travelPlan);
        var template = $"TravelPlanSummary : {serializedPlan}";

        var response = await agent.RunAsync(template, cancellationToken: cancellationToken);

        return response;
    }
}