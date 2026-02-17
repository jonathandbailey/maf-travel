using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Agents.Services;
using Travel.Workflows.Dto;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class ExecutionNode() : Executor<AgentResponse>(NodeNames.ExecutionNodeName)
{
    public override async ValueTask HandleAsync(AgentResponse agentResponse, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.ExecutionNodeName, Guid.NewGuid());

        var toolCalls = agentResponse.ExtractToolCalls();

        foreach (var functionCallContent in toolCalls)
        {
            switch (functionCallContent.Name)
            {
                case PlanningTools.PlanningCompleteToolName:
                    await context.SendMessageAsync(new TravelPlanCompletedCommand(), cancellationToken);
                    break;
                default:
                    await context.SendMessageAsync(functionCallContent, cancellationToken: cancellationToken);
                    break;
            }
        }

        if (toolCalls.Count == 0)
        {
            var error = new ErrorContent("The planner failed to select a required tool.")
            {
                ErrorCode = "MISSING_TOOL_CALL",
                Details = "Required tools: RequestInformation, UpdateTravelPlan, SearchFlights."
            };

            await context.SendMessageAsync(new ChatMessage(ChatRole.Assistant, [error]), cancellationToken: cancellationToken);
        }
    }
}