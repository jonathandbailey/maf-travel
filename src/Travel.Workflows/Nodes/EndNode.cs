using System.Diagnostics;
using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Exceptions;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public partial class EndNode() : Executor(NodeNames.EndNode)
{
    [MessageHandler]
    private async ValueTask HandleAsync(TravelPlanCompletedCommand message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var threadId = await context.GetThreadId(cancellationToken);

        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.EndNode, threadId);

        var travelPlan = await context.GetTravelPlan(cancellationToken);

        travelPlan.Validate(NodeNames.EndNode, threadId);

        activity?.AddTravelPlanStateSnapshotAfter(travelPlan);

        try
        {
            await context.AddEventAsync(new TravelPlanningCompleteEvent(travelPlan), cancellationToken);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            throw new WorkflowException("EndNode failed to emit TravelPlanningCompleteEvent.", NodeNames.EndNode, threadId, exception);
        }
    }
}
