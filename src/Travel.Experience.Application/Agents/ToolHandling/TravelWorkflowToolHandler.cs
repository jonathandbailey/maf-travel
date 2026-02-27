using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Travel.Agents.Dto;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Extensions;
using Travel.Workflows.Interfaces;

namespace Travel.Experience.Application.Agents.ToolHandling;

public sealed class TravelWorkflowToolHandler(IWorkflowFactory workflowFactory) : IToolHandler
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

            if (evt is TravelPlanningCompleteEvent)
            {
                yield return new ToolResultUpdate(
                    new FunctionResultContent(call.CallId, "Travel Planning Complete"));
            }

            if (evt is TravelPlanStatusUpdateEvent travelPlanStatusUpdateEvent)
            {
                yield return new ToolStatusUpdate(travelPlanStatusUpdateEvent.Status);
            }
        }
    }
}
