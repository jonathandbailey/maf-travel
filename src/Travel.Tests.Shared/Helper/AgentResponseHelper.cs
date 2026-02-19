using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Collections;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;

namespace Travel.Tests.Shared.Helper;

public static class AgentResponseHelper
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public static List<FunctionCallContent> FunctionCalls(this AgentResponse response)
    {
        return response.Messages
            .SelectMany(m => m.Contents.OfType<FunctionCallContent>())
            .ToList();
    }

    public static AndConstraint<GenericCollectionAssertions<FunctionCallContent>> ShouldContainCall(
        this GenericCollectionAssertions<FunctionCallContent> assertions, string functionName)
    {
        return assertions.Contain(fc => fc.Name == functionName, 
            $"the function calls should contain a call to '{functionName}'");
    }

    public static AndConstraint<GenericCollectionAssertions<FunctionCallContent>> ShouldHaveArgumentKey(
        this GenericCollectionAssertions<FunctionCallContent> assertions, string argumentKey)
    {
        return assertions.Contain(fc => fc.Arguments != null && fc.Arguments.ContainsKey(argumentKey), 
            $"the function calls should contain an argument with key '{argumentKey}'");
    }

    public static AndConstraint<GenericCollectionAssertions<FunctionCallContent>> ShouldHaveArgumentOfType<T>(
        this GenericCollectionAssertions<FunctionCallContent> assertions, string argumentKey)
    {
        var subject = assertions.Subject.ToList();

        var hasValidArgument = subject.Any(fc =>
        {
            if (fc.Arguments == null || !fc.Arguments.ContainsKey(argumentKey))
                return false;

            try
            {
                var argumentValue = fc.Arguments[argumentKey];
                if (argumentValue == null)
                    return false;

                var jsonString = argumentValue.ToString();
                if (string.IsNullOrEmpty(jsonString))
                    return false;

                JsonSerializer.Deserialize<T>(jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        });

        hasValidArgument.Should().BeTrue(
            $"the function calls should contain an argument with key '{argumentKey}' of type '{typeof(T).Name}'");

        return new AndConstraint<GenericCollectionAssertions<FunctionCallContent>>(assertions);
    }

 

    public static AndConstraint<GenericCollectionAssertions<FunctionCallContent>> ShouldHaveRequiredInputs(
        this GenericCollectionAssertions<FunctionCallContent> assertions, string argumentKey, int expectedCount, List<string> requiredInputs)
    {
        var subject = assertions.Subject.ToList();

        var isValid = subject.Any(fc =>
        {
            if (fc.Arguments == null || !fc.Arguments.ContainsKey(argumentKey))
                return false;

            try
            {
                var argumentValue = fc.Arguments[argumentKey];
                if (argumentValue == null)
                    return false;

                var jsonString = argumentValue.ToString();
                if (string.IsNullOrEmpty(jsonString))
                    return false;

                var dto = JsonSerializer.Deserialize<RequestInformationDto>(jsonString, JsonSerializerOptions);
                if (dto?.RequiredInputs == null)
                    return false;

                if (dto.RequiredInputs.Count != expectedCount)
                    return false;

                return requiredInputs.All(input => dto.RequiredInputs.Contains(input));
            }
            catch
            {
                return false;
            }
        });

        isValid.Should().BeTrue(
            $"the RequestInformationDto should have {expectedCount} required inputs: [{string.Join(", ", requiredInputs)}]");

        return new AndConstraint<GenericCollectionAssertions<FunctionCallContent>>(assertions);
    }
}