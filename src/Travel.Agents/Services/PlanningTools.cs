using System.ComponentModel;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;

namespace Travel.Agents.Services;

public static class PlanningTools
{

    private static readonly Dictionary<string, AIFunction> Tools = new();
    [Description("Request all missing pieces of information from the user in a single batch.")]
    private static string RequestInformation(
        [Description("A consolidated message asking for all missing details")] string message,
        [Description("The collective reasoning for these requests")] string thought,
        [Description("A list of all keys that need to be populated")] List<string> requiredInputs)
        => $"The information requested is: {message}";

    static PlanningTools()
    {
        var function = AIFunctionFactory.Create(RequestInformation);

        Tools[function.Name] = function;
    }

    public static List<AITool> GetDeclarationOnlyTools()
    {
        return Tools.Select(toolMeta => toolMeta.Value.AsDeclarationOnly()).Cast<AITool>().ToList();
    }
}

public static class ExtractingTools
{
    private static readonly Dictionary<string, AIFunction> Tools = new();
    
    [Description("Request More Information")]
    private static string UpdateTravelPlan(
        [Description("The update for the travel plan")] TravelPlanDto travelPlan)
        => $"The travel plan has been updated successfully.";

    static ExtractingTools()
    {
        var function = AIFunctionFactory.Create(UpdateTravelPlan);

        Tools[function.Name] = function;
    }

    public static List<AITool> GetDeclarationOnlyTools()
    {
        return Tools.Select(toolMeta => toolMeta.Value.AsDeclarationOnly()).Cast<AITool>().ToList();
    }
}

public static class ConversationTools
{
    private static readonly Dictionary<string, AIFunction> Tools = new();

    [Description("Plan Travel")]
    private static string PlanTravel(
        [Description("Plan Travel")] string message,
        [Description("Reasoning for selecting this tool based on the user request")] string reasoning,
        [Description("Confidence score for the tool selection, in the range 0.0 to 1.0")] decimal confidenceScore

        )
        => $"The travel plan has been created successfully.";

    static ConversationTools()
    {
        var function = AIFunctionFactory.Create(PlanTravel);

        Tools[function.Name] = function;
    }

    public static List<AITool> GetDeclarationOnlyTools()
    {
        return Tools.Select(toolMeta => toolMeta.Value.AsDeclarationOnly()).Cast<AITool>().ToList();
    }

}