using System.Diagnostics;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Workflows.Common;
using Travel.Workflows.Common.Exceptions;
using Travel.Workflows.Common.Extensions;
using Travel.Workflows.Common.Telemetry;
using Travel.Workflows.TravelPlanCriteria.Dto;
using Travel.Workflows.TravelPlanCriteria.Events;

namespace Travel.Workflows.TravelPlanCriteria.Nodes;


public partial class ExtractionNode(AIAgent agent) : Executor(NodeNames.ExtractionNodeName)
{

    [MessageHandler(Send = [typeof(TravelPlanUpdateCommand)])]
    private async ValueTask HandleAsync(TravelPlanExtractCommand command, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var threadId = await context.GetThreadId(cancellationToken);

        using var activity = TravelWorkflowTelemetry.InvokeNode(NodeNames.ExtractionNodeName, threadId);

        if (string.IsNullOrWhiteSpace(command.Message.Text))
        {
            throw new WorkflowValidationException("ExtractionNode received an empty message.", NodeNames.ExtractionNodeName, threadId);
        }

        activity?.AddNodeAgentInput(command.Message.Text);

        AgentResponse response;
        
        try
        {
            response = await agent.RunAsync(command.Message, cancellationToken: cancellationToken);

            await context.AddEventAsync(new TravelPlanStatusUpdateEvent("Parsing Travel Booking Details...", response.Text, NodeNames.ExtractionNodeName), cancellationToken);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            throw new WorkflowException("ExtractionNode failed to extract travel plan from agent response.", NodeNames.ExtractionNodeName, threadId, exception);
        }

        activity?.AddNodeAgentOutput(response.Text);
        activity?.AddNodeAgentUsage(response, NodeNames.ExtractionNodeName, threadId);

        response.TraceToolCalls(activity);

        var updateCall = response.ExtractToolCalls()
            .FirstOrDefault(c => c.Name == ExtractingToolsHandler.UpdateTravelPlanToolName);

        if (updateCall == null || !updateCall.TryGetArgument<TravelPlanData>(WorkflowConstants.ExtractingNodeUpdatePlanFunctionName, out var details, Json.FunctionCallSerializerOptions))
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Agent did not return expected update_travel_plan tool call.");
            throw new WorkflowException("ExtractionNode: agent response did not contain the expected tool call.", NodeNames.ExtractionNodeName, threadId);
        }

        await context.SendMessageAsync(new TravelPlanUpdateCommand(details), cancellationToken);
    }
}
