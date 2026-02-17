using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;
using Travel.Workflows.Dto;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class PlannerNode(AIAgent agent) : Executor<TravelPlanContextUpdated, AgentResponse>(NodeNames.PlannerNode)
{
    public override async ValueTask<AgentResponse> HandleAsync(TravelPlanContextUpdated message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.PlannerNode, Guid.NewGuid());

        var travelPlan = await context.GetTravelPlan(cancellationToken);

        var serializedPlan = JsonSerializer.Serialize(travelPlan);
        var template = $"TravelPlanSummary : {serializedPlan}";

        var response = await agent.RunAsync(template, cancellationToken: cancellationToken);

        response.TraceToolCalls(activity);

        return response;
    }
}