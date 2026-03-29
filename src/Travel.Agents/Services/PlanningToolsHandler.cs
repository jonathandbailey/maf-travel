using System.ComponentModel;
using Microsoft.Extensions.AI;
using Tools.Registry;
using Travel.Agents.Dto;

namespace Travel.Agents.Services;

public sealed class PlanningToolsHandler : IToolHandler
{
    public const string RequestInformationToolName = "request_information";
    public const string PlanningCompleteToolName = "planning_complete";

    public string ToolName => RequestInformationToolName;

    private static readonly List<AITool> s_tools;

    [Description("Request all missing pieces of information from the user in a single batch.")]
    private static string RequestInformation(
        [Description("The information request containing the message, reasoning, and required inputs")] RequestInformationDto request)
        => $"The information requested is: {request.Message}";

    [Description("Called when planning is complete")]
    private static void PlanningComplete() { }

    static PlanningToolsHandler()
    {
        s_tools =
        [
            AIFunctionFactory.Create(RequestInformation, RequestInformationToolName).AsDeclarationOnly(),
            AIFunctionFactory.Create(PlanningComplete, PlanningCompleteToolName).AsDeclarationOnly()
        ];
    }

    public List<AITool> GetDeclarationOnlyTools() => s_tools;

    public IAsyncEnumerable<ToolHandlerUpdate> ExecuteAsync(
        FunctionCallContent call,
        ToolHandlerContext context,
        CancellationToken cancellationToken)
        => throw new NotSupportedException("Planning tools are executed by workflow nodes, not the tool registry.");
}
