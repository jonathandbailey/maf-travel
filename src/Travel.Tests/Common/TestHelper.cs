using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;
using TravelPlanDto = Travel.Workflows.Dto.TravelPlanDto;

namespace Travel.Tests.Common;

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
            ["travelPlan"] = travelPlanDto
        };

        var toolCallContent = new FunctionCallContent(
            callId: "call_456",
            name: "UpdateTravelPlan",
            arguments: arguments);

        var responseMessage = new ChatMessage(ChatRole.Assistant, [toolCallContent]);
        ((FakeAgent)agent).EnqueueResponse(new AgentResponse([responseMessage]));
        return agent;
    }

    public static TravelPlanDto CreateTravelUpdateRequest()
    {
        var updateDto = new TravelPlanDto
        {
            Origin = "Zurich",
            Destination = "Paris",
            StartDate = DateTime.UtcNow.AddDays(60),
            EndDate = null,
            NumberOfTravelers = 2
        };

        return updateDto;
    }


    public static AIAgent InformationRequest(this AIAgent agent, RequestInformationDto informationRequest)
    {
        var arguments = new Dictionary<string, object?>
        {
            ["request"] = informationRequest
        };

        var toolCallContent = new FunctionCallContent(
            callId: "call_123",
            name: "RequestInformation",
            arguments: arguments);

        var responseMessage = new ChatMessage(ChatRole.Assistant, [toolCallContent]);
        
        
        
        ((FakeAgent)agent).EnqueueResponse(new AgentResponse([responseMessage]));

        return agent;
    }

    public static RequestInformationDto CreateInformationRequest()
    {
        var informationRequest =
            new RequestInformationDto(
                "Travel Plan Information is missing.","End Date is requird to to complete the travel planning.",
                [ "EndDate"]);

        return informationRequest;
    }
}