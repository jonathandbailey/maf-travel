using System.Text.Json;
using FluentAssertions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;

namespace Travel.Tests.Common;

public static class ResponseHelper
{
    public static FunctionCallValidator ValidateFunctionCalls(AgentResponse response)
    {
        return new FunctionCallValidator(response);
    }

    public static string? GetFunctionCallArgument(AgentResponse response, string functionName, string argumentName)
    {
        response.Messages.Should().ContainSingle();
        var functionCalls = response.Messages.Single().Contents.OfType<FunctionCallContent>().ToList();
        
        var functionCall = functionCalls.Should()
            .ContainSingle(f => f.Name == functionName, $"response should contain a call to {functionName}")
            .Subject;

        functionCall.Arguments.Should().NotBeNull()
            .And.ContainKey(argumentName, $"function {functionName} should have argument '{argumentName}'");

        return functionCall.Arguments![argumentName]?.ToString();
    }

    public static TravelPlanDto DeserializeTravelPlan(AgentResponse response, string functionName = "UpdateTravelPlan")
    {
        response.Messages.Should().ContainSingle();
        var functionCalls = response.Messages.Single().Contents.OfType<FunctionCallContent>().ToList();
        
        var functionCall = functionCalls.Should()
            .ContainSingle(f => f.Name == functionName, $"response should contain a call to {functionName}")
            .Subject;

        functionCall.Arguments.Should().NotBeNull()
            .And.ContainKey("travelPlan", $"function {functionName} should have argument 'travelPlan'");

        var travelPlanJson = functionCall.Arguments!["travelPlan"];
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        if (travelPlanJson is JsonElement jsonElement)
        {
            var travelPlan = JsonSerializer.Deserialize<TravelPlanDto>(jsonElement.GetRawText(), options);
            travelPlan.Should().NotBeNull("travelPlan argument should be deserializable");
            return travelPlan!;
        }
        else
        {
            var json = JsonSerializer.Serialize(travelPlanJson);
            var travelPlan = JsonSerializer.Deserialize<TravelPlanDto>(json, options);
            travelPlan.Should().NotBeNull("travelPlan argument should be deserializable");
            return travelPlan!;
        }
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

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        if (travelPlanJson is JsonElement jsonElement)
        {
            travelPlan = JsonSerializer.Deserialize<TravelPlanDto>(jsonElement.GetRawText(), options);
        }
        else
        {
            var json = JsonSerializer.Serialize(travelPlanJson);
            travelPlan = JsonSerializer.Deserialize<TravelPlanDto>(json, options);
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

    public FunctionCallAssertion WithMessage(string message)
    {
        _functionCall.Arguments.Should().NotBeNull()
            .And.ContainKey("message", $"function {_functionCall.Name} should have argument 'message'");

        var actualMessage = _functionCall.Arguments!["message"]?.ToString();
        actualMessage.Should().Be(message, $"function {_functionCall.Name} message should be '{message}'");

        return this;
    }

    public FunctionCallAssertion WithThought(string thought)
    {
        _functionCall.Arguments.Should().NotBeNull()
            .And.ContainKey("thought", $"function {_functionCall.Name} should have argument 'thought'");

        var actualThought = _functionCall.Arguments!["thought"]?.ToString();
        actualThought.Should().Be(thought, $"function {_functionCall.Name} thought should be '{thought}'");

        return this;
    }

    public FunctionCallAssertion WithRequiredInputs(params string[] requiredInputs)
    {
        _functionCall.Arguments.Should().NotBeNull();
   
        var requiredInputsArg = _functionCall.Arguments["request"];
        RequestInformationDto? actualRequiredInputs = null;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        if (requiredInputsArg is JsonElement jsonElement)
        {
            actualRequiredInputs = JsonSerializer.Deserialize<RequestInformationDto>(jsonElement.GetRawText(), options);
        }

        if(actualRequiredInputs == null)
        {
            throw new ArgumentNullException(nameof(requiredInputs), "requiredInputs argument should not be null");
        }

        actualRequiredInputs.RequiredInputs.Should().NotBeNull($"requiredInputs argument should be deserializable");
        actualRequiredInputs.RequiredInputs.Should().BeEquivalentTo(requiredInputs, $"function {_functionCall.Name} should have requiredInputs matching expected values");

        return this;
    }

    public FunctionCallAssertion WithMessageContaining(string substring)
    {
        _functionCall.Arguments.Should().NotBeNull()
            .And.ContainKey("message", $"function {_functionCall.Name} should have argument 'message'");

        var actualMessage = _functionCall.Arguments!["message"]?.ToString();
        actualMessage.Should().Contain(substring, $"function {_functionCall.Name} message should contain '{substring}'");

        return this;
    }

    public FunctionCallAssertion WithThoughtContaining(string substring)
    {
        _functionCall.Arguments.Should().NotBeNull()
            .And.ContainKey("thought", $"function {_functionCall.Name} should have argument 'thought'");

        var actualThought = _functionCall.Arguments!["thought"]?.ToString();
        actualThought.Should().Contain(substring, $"function {_functionCall.Name} thought should contain '{substring}'");

        return this;
    }

    public FunctionCallValidator And()
    {
        return _validator;
    }
}