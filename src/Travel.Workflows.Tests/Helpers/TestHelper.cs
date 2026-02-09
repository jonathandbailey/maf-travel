using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Workflows.Planning.Dto;
using Travel.Workflows.Planning.Services;

namespace Travel.Workflows.Tests.Helpers;

public static  class TestHelper
{
    public static AIAgent FinalizeTravelPlan(this AIAgent agent)
    {
        var toolCallContent = new FunctionCallContent(
            callId: "call_451",
            name: "finalize_travel_plan");

        var responseMessage = new ChatMessage(ChatRole.Assistant, [toolCallContent]);
        
        ((FakeAgent)agent).EnqueueResponse(new AgentResponse([responseMessage]));
        
        return agent;
    }

    public static AIAgent UpdateTravelPlan(this AIAgent agent,  TravelPlanDto travelPlanDto)
    {
     
        var arguments = new Dictionary<string, object?>
        {
            ["travelPlanUpdate"] = travelPlanDto
        };

        var toolCallContent = new FunctionCallContent(
            callId: "call_456",
            name: "update_travel_plan",
            arguments: arguments);

        var responseMessage = new ChatMessage(ChatRole.Assistant, [toolCallContent]);
        ((FakeAgent)agent).EnqueueResponse(new AgentResponse([responseMessage]));
        return agent;
    }

    public static TravelPlanDto CreateTravelUpdateRequest()
    {
        var updateDto = new TravelPlanDto
        {
            Origin = "Seattle",
            Destination = "Tokyo",
            StartDate = DateTimeOffset.UtcNow.AddDays(60),
            EndDate = DateTimeOffset.UtcNow.AddDays(67)
        };

        return updateDto;
    }


    public static AIAgent InformationRequest(this AIAgent agent, InformationRequestDetails informationRequest)
    {
        var arguments = new Dictionary<string, object?>
        {
            ["informationRequest"] = informationRequest
        };

        var toolCallContent = new FunctionCallContent(
            callId: "call_123",
            name: "information_request",
            arguments: arguments);

        var responseMessage = new ChatMessage(ChatRole.Assistant, [toolCallContent]);
        
        
        
        ((FakeAgent)agent).EnqueueResponse(new AgentResponse([responseMessage]));

        return agent;
    }

    public static InformationRequestDetails CreateInformationRequest()
    {
        var informationRequest =
            new InformationRequestDetails(
                "Travel Plan Information is missing.",
                ["Origin", "Destination", "StartDate", "EndDate"]);

        return informationRequest;
    }
}