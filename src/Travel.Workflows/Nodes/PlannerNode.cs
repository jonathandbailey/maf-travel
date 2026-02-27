using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System.Diagnostics;
using System.Text.Json;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Exceptions;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public partial class PlannerNode(AIAgent agent) : Executor(NodeNames.PlannerNode)
{
    [MessageHandler(Send = [typeof(AgentResponse)])]
    private async ValueTask HandleAsync(TravelPlanContextUpdated message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var threadId = await context.GetThreadId(cancellationToken);

        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.PlannerNode, threadId);

        var travelPlan = await context.GetTravelPlan(cancellationToken);

        var serializedPlan = JsonSerializer.Serialize(new TravelPlanSummary(travelPlan));

        if (string.IsNullOrWhiteSpace(serializedPlan) || serializedPlan == "{}")
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Serialized travel plan is empty.");
            throw new WorkflowValidationException("PlannerNode: serialized travel plan is empty.", NodeNames.PlannerNode, threadId);
        }

        activity?.AddNodeAgentInput(serializedPlan);

        AgentResponse response;

        try
        {
            response = await agent.RunAsync(serializedPlan, cancellationToken: cancellationToken);

            await context.AddEventAsync(new TravelPlanStatusUpdateEvent("Planning Next Actions...", response.Text, NodeNames.PlannerNode), cancellationToken);

        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            throw new WorkflowException("PlannerNode failed to get a response from the planning agent.", NodeNames.PlannerNode, threadId, exception);
        }

        activity?.AddNodeAgentOutput(response.Text);
        activity?.AddNodeAgentUsage(response, NodeNames.PlannerNode, threadId);

        response.TraceToolCalls(activity);

        await context.SendMessageAsync(response, cancellationToken: cancellationToken);
    }
}
