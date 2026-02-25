using System.Diagnostics;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Exceptions;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public partial class ExecutionNode() : Executor(NodeNames.ExecutionNodeName)
{
    private const string ThePlannerFailedToSelectARequiredTool = "The planner failed to select a required tool.";

    [MessageHandler(Send = [typeof(RequestInformationCommand), typeof(TravelPlanCompletedCommand)])]
    private async ValueTask HandleAsync(AgentResponse agentResponse, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var threadId = await context.GetThreadId(cancellationToken);

        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.ExecutionNodeName, threadId);

        var toolCalls = agentResponse.ExtractToolCalls();

        activity?.AddNodeAgentOutput(agentResponse.Text);
        activity?.AddNodeAgentUsage(agentResponse);

        agentResponse.TraceToolCalls(activity);

        if (toolCalls.Count == 0)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ThePlannerFailedToSelectARequiredTool);
            throw new WorkflowException(ThePlannerFailedToSelectARequiredTool, NodeNames.ExecutionNodeName, threadId);
        }

        var toolCall = toolCalls[0];

        switch (toolCall.Name)
        {
            case PlanningTools.PlanningCompleteToolName:
                await context.SendMessageAsync(new TravelPlanCompletedCommand(), cancellationToken);
                break;

            case PlanningTools.RequestInformationToolName:
                if (!toolCall.TryGetArgument<RequestInformationDto>(WorkflowConstants.InformationRequestFunctionArgumentName, out var dto, Json.FunctionCallSerializerOptions))
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "request_information tool call missing required arguments.");
                    throw new WorkflowException("ExecutionNode: request_information tool call missing required arguments.", NodeNames.ExecutionNodeName, threadId);
                }
                await context.SendMessageAsync(new RequestInformationCommand(dto), cancellationToken);
                break;

            default:
                activity?.SetStatus(ActivityStatusCode.Error, $"Unrecognised tool call: {toolCall.Name}");
                throw new WorkflowException($"ExecutionNode: unrecognised tool call '{toolCall.Name}'.", NodeNames.ExecutionNodeName, threadId);
        }
    }
}