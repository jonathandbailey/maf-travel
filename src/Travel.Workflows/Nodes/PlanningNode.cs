using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using System.Text.Json;
using Travel.Workflows.Dto;

namespace Travel.Workflows.Nodes;

public class PlanningNode(AIAgent agent) : ReflectingExecutor<PlanningNode>("Planning"), 
    IMessageHandler<TravelPlanContextUpdated, AgentResponse>
{

    public async ValueTask<AgentResponse> HandleAsync(TravelPlanContextUpdated message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var fileContent = await context.ReadStateAsync<TravelPlanDto>("TravelPlan", scopeName: "TravelPlanScope", cancellationToken: cancellationToken)
                          ?? throw new InvalidOperationException("File content state not found");

        var serializedPlan = JsonSerializer.Serialize(fileContent);
        var template = $"TravelPlanSummary : {serializedPlan}";

        var response = await agent.RunAsync(template, cancellationToken: cancellationToken);

        return response;
    }
}