using Microsoft.Extensions.AI;
using System.ComponentModel;

namespace Travel.Experience.Application.Agents;

public static class ConversationAgentTools
{
    public const string RequestInformationToolName = "travel_booking_details";
  
    private static readonly Dictionary<string, AIFunction> Tools = new();

    [Description("Gathers the required travel booking details when a user wants to plan a vacation.")]
    private static string TravelBookingDetails(
        [Description("The user's travel planning request")] string request)
        => $"The information requested is: {request}";

    [Description("Called when planning is complete")]
    private static void PlanningComplete() { }



    static ConversationAgentTools()
    {
        var function = AIFunctionFactory.Create(TravelBookingDetails, RequestInformationToolName);

        Tools[function.Name] = function;
    }

    public static List<AITool> GetDeclarationOnlyTools()
    {
        return Tools.Select(toolMeta => toolMeta.Value.AsDeclarationOnly()).Cast<AITool>().ToList();
    }
}