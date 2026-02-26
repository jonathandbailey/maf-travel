using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using Travel.Agents.Dto;
using Travel.Experience.Application.Agents;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Extensions;
using Travel.Workflows.Interfaces;

namespace Travel.Experience.Application.Agents.ToolHandling;

public sealed class TravelWorkflowToolHandler(IWorkflowFactory workflowFactory) : IConversationToolHandler
{
    private const string ExecutingTravelWorkflow = "Executing Travel Workflow...";

    public string ToolName => ConversationAgentTools.RequestInformationToolName;

    public async IAsyncEnumerable<ToolHandlerUpdate> ExecuteAsync(
        FunctionCallContent call,
        ToolHandlerContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return new ToolStatusUpdate(ExecutingTravelWorkflow);

        if (!Guid.TryParse(context.ThreadId, out var threadId))
            yield break;

        if (!call.TryGetArgument<string>("request", out var details, Json.FunctionCallSerializerOptions))
            yield break;

        var workflow = await workflowFactory.Create();

        var request = new TravelWorkflowRequest(
            new ChatMessage(ChatRole.User, details),
            threadId,
            new TravelPlanDto());

        await foreach (var evt in workflow.WatchStreamAsync(request).WithCancellation(cancellationToken))
        {
            if (evt is RequestInfoEvent requestInfoEvent)
            {
                if (requestInfoEvent.Data is not ExternalRequest externalRequest) continue;

                var informationRequest = externalRequest.Data.AsType(typeof(InformationRequest));

                yield return new ToolResultUpdate(
                    new FunctionResultContent(call.CallId, informationRequest));
            }
            else if (evt is TravelPlanningCompleteEvent)
            {
                yield return new ToolResultUpdate(
                    new FunctionResultContent(call.CallId, "Travel Planning Complete"));
            }
        }
    }
}
