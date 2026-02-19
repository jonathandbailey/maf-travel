using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;
using Travel.Tests.Common;
using Travel.Tests.Shared.Helper;

namespace Travel.Tests.Unit;

public class Agents
{
    private const string Destination = "Paris";
    private const int NumberOfTravelers = 2;
    private static readonly DateTime DepartureDate = new(2026, 5, 1);

    private const string RequestInformationToolName = "request_information";
    private const string ToolCallArgumentKey = "request";

    private readonly List<string> _expectedKeys = ["Origin", "ReturnDate"];

    private readonly TravelPlanDto _travePlanState = new(destination: Destination, startDate: DepartureDate, numberOfTravelers: NumberOfTravelers);


    [Fact]
    public async Task PlanningAgent_ShouldRequestMissingInformationToolCall_WhenTravelPlanIsIncomplete()
    {
        var requestInfoDto = new RequestInformationDto(
            Message: "Please provide the missing information",
            Thought: "Need to request missing travel information",
            RequiredInputs: ["Origin", "ReturnDate"]
        );

        var agent = await AgentFactoryHelper.CreateMockPlanningAgent(requestInfoDto);

        var chatMessage = CreateTravelPlanMessage(_travePlanState);

        var response = await agent.RunAsync(chatMessage);

        response.FunctionCalls()
            .Should().HaveCount(1).And
            .ShouldContainCall(RequestInformationToolName).And
            .ShouldHaveArgumentKey(ToolCallArgumentKey).And
            .ShouldHaveArgumentOfType<RequestInformationDto>(ToolCallArgumentKey).And
            .ShouldHaveRequiredInputs(ToolCallArgumentKey, _expectedKeys.Count, _expectedKeys);


    }

    public static ChatMessage CreateTravelPlanMessage(TravelPlanDto travelPlan)
    {
        var serializedPlan = JsonSerializer.Serialize(travelPlan);
        var template = $"TravelPlanSummary : {serializedPlan}";
        return new ChatMessage(ChatRole.User, template);
    }
}