using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using System.Text.Json;
using Travel.Workflows.Dto;

namespace Travel.Workflows.Nodes;

public class PlanningNode(AIAgent agent) : ReflectingExecutor<PlanningNode>("Planning"), IMessageHandler<TravelPlanDto, AgentResponse>
{
    public async ValueTask<AgentResponse> HandleAsync(TravelPlanDto travelPlan, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var serializedPlan = JsonSerializer.Serialize(travelPlan);
        var template = $"TravelPlanSummary : {serializedPlan}";

        var response = await agent.RunAsync(template, cancellationToken: cancellationToken);

        return response;
    }
}