using System.Diagnostics;
using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Common;
using Travel.Workflows.Exceptions;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;
using Travel.Workflows.TravelPlanCriteria.Dto;

namespace Travel.Workflows.TravelPlanCriteria.Nodes;

public partial class StartNode() : Executor(NodeNames.StartNodeName)
{
    [MessageHandler(Send = [typeof(TravelPlanExtractCommand)])]
    private async ValueTask HandleAsync(WorkflowRunRequest request, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.StartNodeName, request.ThreadId);

        request.Validate();

        activity?.AddNodeAgentInput(request.Message.Text);

        try
        {
            var travelPlan = request.TravelPlan;

            await context.SetTravelPlan(travelPlan, cancellationToken);

            await context.SetThreadId(request.ThreadId, cancellationToken);

            activity?.AddTravelPlanStateSnapshotAfter(travelPlan);

            await context.SendMessageAsync(new TravelPlanExtractCommand(request.Message), cancellationToken);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            throw new WorkflowException("StartNode failed to initialize workflow state.", NodeNames.StartNodeName, request.ThreadId, exception);
        }
    }
}
