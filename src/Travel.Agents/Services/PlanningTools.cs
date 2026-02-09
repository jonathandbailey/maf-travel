using System.ComponentModel;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;

namespace Travel.Agents.Services;

public static class PlanningTools
{

    private static readonly Dictionary<string, AIFunction> Tools = new();

    [Description("Request More Information")]
    private static string RequestInformation(
        [Description("The details of the information being requested")] string message,
        [Description("The reasoning behind the information request")] string thought,
        [Description("A list of the parameters required for the information request")] List<string> requiredInputs)
        => $"The information requested is: {message}";

    [Description("Request More Information")]
    private static string UpdateTravelPlan(
        [Description("The update for the travel plan")] TravelPlanDto travelPlan)
        => $"The travel plan has been updated successfully.";

    static PlanningTools()
    {
        var function = AIFunctionFactory.Create(RequestInformation);

        Tools[function.Name] = function;

        function = AIFunctionFactory.Create(UpdateTravelPlan);

        Tools[function.Name] = function;
    }

    public static List<AITool> GetDeclarationOnlyTools()
    {
        return Tools.Select(toolMeta => toolMeta.Value.AsDeclarationOnly()).Cast<AITool>().ToList();
    }

    public static List<AIFunction> GetDeclarationOnlyFunctions()
    {
        return Tools.Select(toolMeta => toolMeta.Value.AsDeclarationOnly()).Cast<AIFunction>().ToList();
    }

    public static List<AITool> GetTools()
    {
        return Tools.Select(toolMeta => toolMeta.Value).Cast<AITool>().ToList();
    }



    public static AIFunction Get(string name)
    {
        return Tools[name];
    }
}