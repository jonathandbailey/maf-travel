using System.ComponentModel;
using Microsoft.Extensions.AI;
using Tools.Registry;
using Travel.Agents.Dto;

namespace Travel.Agents.Services;

public sealed class ExtractingToolsHandler : IToolHandler
{
    public const string UpdateTravelPlanToolName = "update_travel_plan";

    public string ToolName => UpdateTravelPlanToolName;

    private static readonly List<AITool> s_tools;

    [Description("Update the travel plan with information extracted from the user's message")]
    private static string UpdateTravelPlan(
        [Description("The update for the travel plan")] TravelPlanData travelPlan)
        => "The travel plan has been updated successfully.";

    static ExtractingToolsHandler()
    {
        s_tools =
        [
            AIFunctionFactory.Create(UpdateTravelPlan, UpdateTravelPlanToolName).AsDeclarationOnly()
        ];
    }

    public List<AITool> GetDeclarationOnlyTools() => s_tools;

    public IAsyncEnumerable<ToolHandlerUpdate> ExecuteAsync(
        FunctionCallContent call,
        ToolHandlerContext context,
        CancellationToken cancellationToken)
        => throw new NotSupportedException("Extracting tools are executed by workflow nodes, not the tool registry.");
}
