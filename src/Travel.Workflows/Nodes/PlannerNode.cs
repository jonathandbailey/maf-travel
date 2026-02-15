using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Travel.Workflows.Dto;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class PlannerNode(AIAgent agent) : ReflectingExecutor<PlannerNode>("Planner"), 
    IMessageHandler<TravelPlanContextUpdated, AgentResponse>
{

    public async ValueTask<AgentResponse> HandleAsync(TravelPlanContextUpdated message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode("Planner", Guid.NewGuid());


        var travelPlan = await context.GetTravelPlan(cancellationToken);

        var serializedPlan = JsonSerializer.Serialize(travelPlan);
        var template = $"TravelPlanSummary : {serializedPlan}";

        var response = await agent.RunAsync(template, cancellationToken: cancellationToken);

        foreach (var responseMessage in response.Messages)
        {
            foreach (var content in responseMessage.Contents)
            {
                if (content is FunctionCallContent functionCallContent)
                {
                   using var toolCallActivity = TravelWorkflowTelemetry.ToolCall(functionCallContent.Name, functionCallContent.Arguments, activity);
                }
            }
        }

        return response;
    }
}