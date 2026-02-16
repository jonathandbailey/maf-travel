using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Travel.Agents.Services;
using Travel.Workflows.Dto;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class ExecutionNode() : ReflectingExecutor<ExecutionNode>("Execution"), IMessageHandler<AgentResponse>
{
    public async ValueTask HandleAsync(AgentResponse agentResponse, IWorkflowContext context, CancellationToken cancellationToken)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode("Execution", Guid.NewGuid());


        var toolCalls = new List<FunctionCallContent>();

        foreach (var msg in agentResponse.Messages)
        {
            foreach (var content in msg.Contents)
            {
                if (content is FunctionCallContent functionCall)
                {
                    toolCalls.Add(functionCall);

                    await context.SendMessageAsync(functionCall, cancellationToken: cancellationToken);

                    if (functionCall.Name == PlanningTools.PlanningCompleteToolName)
                    {
                        await context.SendMessageAsync(new TravelPlanCompletedCommand(), cancellationToken);
                    }
                }
                
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