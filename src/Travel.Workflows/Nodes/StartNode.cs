using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;
using Travel.Workflows.Dto;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class StartNode() : Executor<TravelWorkflowRequest, ChatMessage>(NodeNames.StartNodeName)
{
    public override async ValueTask<ChatMessage> HandleAsync(TravelWorkflowRequest request, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.StartNodeName, request.ThreadId);

        var travelPlan = request.TravelPlan ?? new TravelPlanDto();
        
        await context.SetTravelPlan(travelPlan, cancellationToken);

        activity?.AddTravelPlanStateSnapshotBefore(travelPlan);

        return request.Message;
    }
}