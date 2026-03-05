using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Agents.Tools;
using Travel.Agents.Dto;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Extensions;
using Travel.Workflows.Interfaces;

namespace Travel.Experience.Application.Agents;

public sealed class TravelWorkflowToolHandler(Func<IWorkflowFactory> workflowFactoryProvider) : IToolHandler
{
    public const string RequestInformationToolName = "travel_booking_details";

    private const string ExecutingTravelWorkflow = "Executing Travel Workflow...";

    private static readonly Dictionary<string, AIFunction> s_tools;

    [Description("Gathers the required travel booking details (origin, departure date etc.) when a user wants to plan a vacation.")]
    private static string TravelBookingDetails(
        [Description("The user's travel planning request")] string request)
        => $"The information requested is: {request}";

    static TravelWorkflowToolHandler()
    {
        s_tools = [];
        var function = AIFunctionFactory.Create(TravelBookingDetails, RequestInformationToolName);
        s_tools[function.Name] = function;
    }

    public string ToolName => RequestInformationToolName;

    public List<AITool> GetDeclarationOnlyTools()
    {
        return s_tools.Select(toolMeta => toolMeta.Value.AsDeclarationOnly()).Cast<AITool>().ToList();
    }

    public async IAsyncEnumerable<ToolHandlerUpdate> ExecuteAsync(
        FunctionCallContent call,
        ToolHandlerContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var threadId = context.ThreadId;

        if (!call.TryGetArgument<string>("request", out var details, Json.FunctionCallSerializerOptions))
            yield break;

        var workflow = await workflowFactoryProvider().Create();

        var request = new TravelWorkflowRequest(
            new ChatMessage(ChatRole.User, details),
            threadId,
            new TravelPlanState());

        await foreach (var evt in workflow.WatchStreamAsync(request, cancellationToken))
        {
            if (evt is RequestInfoEvent requestInfoEvent)
            {
                if (requestInfoEvent.Data is not ExternalRequest externalRequest) continue;

                var informationRequest = externalRequest.Data.AsType(typeof(InformationRequest));

                yield return new ToolResultUpdate(
                    new FunctionResultContent(call.CallId, informationRequest));
            }

            if (evt is TravelPlanningCompleteEvent)
            {
                yield return new ToolResultUpdate(
                    new FunctionResultContent(call.CallId, "Travel Planning Complete"));
            }

            if (evt is TravelPlanStatusUpdateEvent travelPlanStatusUpdateEvent)
            {
                yield return new ToolStatusUpdate(travelPlanStatusUpdateEvent.Status, travelPlanStatusUpdateEvent.Thought, travelPlanStatusUpdateEvent.Source);
            }

            if (evt is TravelPlanUpdateEvent travelPlanUpdateEvent)
            {
                yield return new ToolStateSnapshotUpdate("TravelPlanUpdate", travelPlanUpdateEvent.TravelPlanState);
            }

            if (evt is ExecutorFailedEvent or WorkflowErrorEvent)
            {
                yield return new ToolErrorUpdate("Something went wrong while planning your trip. Please try again.");
                yield break;
            }
        }
    }
}
