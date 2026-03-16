using System.Diagnostics;
using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Common;
using Travel.Workflows.Exceptions;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;
using Travel.Workflows.TravelPlanCriteria.Dto;
using Travel.Workflows.TravelPlanCriteria.Events;

namespace Travel.Workflows.TravelPlanCriteria.Nodes;

public partial class UpdateNode() : Executor(NodeNames.UpdateNode)
{
    [MessageHandler(Send = [typeof(TravelPlanContextUpdated)])]
    private async ValueTask HandleAsync(TravelPlanUpdateCommand command, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var threadId = await context.GetThreadId(cancellationToken);

        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.UpdateNode, threadId);

        if (command.Data == null)
        {
            throw new WorkflowValidationException("UpdateNode received a command with a null TravelPlan.", NodeNames.UpdateNode, threadId);
        }

        try
        {
            var travelPlan = await context.GetTravelPlan(cancellationToken);

            activity?.AddTravelPlanStateSnapshotBefore(travelPlan);

            travelPlan.ApplyPatch(command.Data);

            activity?.AddTravelPlanStateSnapshotAfter(travelPlan);

            await context.SetTravelPlan(travelPlan, cancellationToken);

            await context.AddEventAsync(new TravelPlanUpdateEvent(travelPlan), cancellationToken);

            await context.SendMessageAsync(new TravelPlanContextUpdated(), cancellationToken);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            throw new WorkflowException("UpdateNode failed to apply travel plan update.", NodeNames.UpdateNode, threadId, exception);
        }
    }
}
