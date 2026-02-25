using System.Diagnostics;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Workflows.Dto;
using Travel.Workflows.Exceptions;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class StartNode() : Executor<TravelWorkflowRequest, ChatMessage>(NodeNames.StartNodeName)
{
    public override async ValueTask<ChatMessage> HandleAsync(TravelWorkflowRequest request, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.StartNodeName, request.ThreadId);

        Validate(request);

        activity?.AddNodeAgentInput(request.Message.Text);

        try
        {
            var travelPlan = request.TravelPlan;

            await context.SetTravelPlan(travelPlan, cancellationToken);

            await context.SetThreadId(request.ThreadId, cancellationToken);

            activity?.AddTravelPlanStateSnapshotAfter(travelPlan);

            return request.Message;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            throw new WorkflowException("StartNode failed to initialize workflow state.", NodeNames.StartNodeName, request.ThreadId, exception);
        }
    }

    private static void Validate(TravelWorkflowRequest request)
    {
        if (request.ThreadId == Guid.Empty)
        {
            throw new WorkflowValidationException("ThreadId cannot be Guid.Empty.", NodeNames.StartNodeName, Guid.Empty);
        }

        if (request.Message.Role != ChatRole.User)
        {
            throw new WorkflowValidationException("Message must have Role 'User'.", NodeNames.StartNodeName, request.ThreadId);
        }

        if (string.IsNullOrEmpty(request.Message.Text))
        {
            throw new WorkflowValidationException("Message Text must have Content.", NodeNames.StartNodeName, request.ThreadId);
        }
    }
}