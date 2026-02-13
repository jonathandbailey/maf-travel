using System.ComponentModel;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;

namespace Travel.Agents.Services;

public static class PlanningTools
{

    public const string RequestInformationToolName = "request_information";
   
    
    private static readonly Dictionary<string, AIFunction> Tools = new();

    [Description("Request all missing pieces of information from the user in a single batch.")]
    private static string RequestInformation(
        [Description("The information request containing the message, reasoning, and required inputs")] RequestInformationDto request)
        => $"The information requested is: {request.Message}";

    [Description("Called when planning is complete")]
    private static void PlanningComplete(){}
        
        

    static PlanningTools()
    {
        var function = AIFunctionFactory.Create(RequestInformation, RequestInformationToolName);

        Tools[function.Name] = function;

        var planningCompleteFunction = AIFunctionFactory.Create(PlanningComplete);
        
        Tools[planningCompleteFunction.Name] = planningCompleteFunction;
    }

    public static List<AITool> GetDeclarationOnlyTools()
    {
        return Tools.Select(toolMeta => toolMeta.Value.AsDeclarationOnly()).Cast<AITool>().ToList();
    }
}

public static class ExtractingTools
{
    public const string UpdateTravelPlanToolName = "update_travel_plan";    

    private static readonly Dictionary<string, AIFunction> Tools = new();
    
    [Description("Request More Information")]
    private static string UpdateTravelPlan(
        [Description("The update for the travel plan")] TravelPlanDto travelPlan)
        => $"The travel plan has been updated successfully.";

    static ExtractingTools()
    {
        var function = AIFunctionFactory.Create(UpdateTravelPlan, UpdateTravelPlanToolName);

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