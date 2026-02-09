using System.Text.Json;
using FluentAssertions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;

namespace Travel.Workflows.Tests.Integration.Helper;

public static class ResponseHelper
{
    public static FunctionCallValidator ValidateFunctionCalls(AgentResponse response)
    {
        return new FunctionCallValidator(response);
    }
}

public class FunctionCallValidator
{
    private readonly AgentResponse _response;
    private readonly List<FunctionCallContent> _functionCalls;

    public FunctionCallValidator(AgentResponse response)
    {
        _response = response;
        
        _response.Messages.Should().ContainSingle();
        _functionCalls = _response.Messages.Single().Contents.OfType<FunctionCallContent>().ToList();
    }

    public FunctionCallAssertion ShouldContainCall(string functionName)
    {
        var functionCall = _functionCalls.Should()
            .ContainSingle(f => f.Name == functionName, $"response should contain a call to {functionName}")
            .Subject;

        return new FunctionCallAssertion(functionCall, this);
    }

    public FunctionCallValidator ShouldHaveCallCount(int expectedCount)
    {
        _functionCalls.Should().HaveCount(expectedCount);
        return this;
    }
}

public class FunctionCallAssertion
{
    private readonly FunctionCallContent _functionCall;
    private readonly FunctionCallValidator _validator;

    public FunctionCallAssertion(FunctionCallContent functionCall, FunctionCallValidator validator)
    {
        _functionCall = functionCall;
        _validator = validator;
    }

    public FunctionCallAssertion WithArgument(string key)
    {
        _functionCall.Arguments.Should().NotBeNull()
            .And.ContainKey(key, $"function {_functionCall.Name} should have argument '{key}'");
        return this;
    }

    public FunctionCallAssertion WithTravelPlan(Action<TravelPlanDto> validation)
    {
        _functionCall.Arguments.Should().NotBeNull()
            .And.ContainKey("travelPlan", $"function {_functionCall.Name} should have argument 'travelPlan'");

        var travelPlanJson = _functionCall.Arguments!["travelPlan"];
        TravelPlanDto? travelPlan;

        if (travelPlanJson is JsonElement jsonElement)
        {
            travelPlan = JsonSerializer.Deserialize<TravelPlanDto>(jsonElement.GetRawText());
        }
        else
        {
            var json = JsonSerializer.Serialize(travelPlanJson);
            travelPlan = JsonSerializer.Deserialize<TravelPlanDto>(json);
        }

        travelPlan.Should().NotBeNull($"travelPlan argument should be deserializable");
        validation(travelPlan!);

        return this;
    }

    public FunctionCallAssertion WithDestination(string destination)
    {
        return WithTravelPlan(plan =>
            plan.Destination.Should().Be(destination, $"travel plan destination should be {destination}"));
    }

    public FunctionCallAssertion WithStartDate(DateTime startDate)
    {
        return WithTravelPlan(plan =>
            plan.StartDate.Should().Be(startDate, $"travel plan start date should be {startDate:yyyy-MM-dd}"));
    }

    public FunctionCallAssertion WithEndDate(DateTime endDate)
    {
        return WithTravelPlan(plan =>
            plan.EndDate.Should().Be(endDate, $"travel plan end date should be {endDate:yyyy-MM-dd}"));
    }

    public FunctionCallAssertion WithOrigin(string origin)
    {
        return WithTravelPlan(plan =>
            plan.Origin.Should().Be(origin, $"travel plan origin should be {origin}"));
    }

    public FunctionCallAssertion WithNumberOfTravelers(int numberOfTravelers)
    {
        return WithTravelPlan(plan =>
            plan.NumberOfTravelers.Should().Be(numberOfTravelers, $"travel plan should have {numberOfTravelers} travelers"));
    }

    public FunctionCallValidator And()
    {
        return _validator;
    }
}