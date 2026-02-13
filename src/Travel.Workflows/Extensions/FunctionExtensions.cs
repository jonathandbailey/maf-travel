using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Travel.Workflows.Extensions;

public static class FunctionExtensions
{
    public static bool TryGetArgument<T>(
        this FunctionCallContent functionCall,
        string key,
        [NotNullWhen(true)] out T? value,
        JsonSerializerOptions? options = null)
    {
        if (functionCall.Arguments?.TryGetValue(key, out var obj) == true)
        {
            if (obj is JsonElement jsonElement)
            {
                value = JsonSerializer.Deserialize<T>(jsonElement, options);
                return value != null;
            }

            var element = JsonSerializer.SerializeToElement(obj, options);
            value = JsonSerializer.Deserialize<T>(element, options);
            return value != null;
        }

        value = default;
        return false;
    }

    public static bool TryGetFunctionArgument<T>(
        this AgentResponse response,
        string key,
        [NotNullWhen(true)] out T? value,
        JsonSerializerOptions? options = null)
    {
        foreach (var message in response.Messages)
        {
            foreach (var content in message.Contents)
            {
                if (content is FunctionCallContent functionCall)
                {
                    if (functionCall.TryGetArgument<T>(key, out value, options))
                    {
                        return true;
                    }
                }
            }
        }

        value = default;
        return false;
    }
}