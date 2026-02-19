using System.Text.Json;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;

namespace Travel.Tests.Shared.Helper;

public static class MessageHelper
{
    public static ChatMessage CreateTravelPlanMessage(TravelPlanDto travelPlan)
    {
        var serializedPlan = JsonSerializer.Serialize(travelPlan);
        var template = $"TravelPlanSummary : {serializedPlan}";
        return new ChatMessage(ChatRole.User, template);
    }
}