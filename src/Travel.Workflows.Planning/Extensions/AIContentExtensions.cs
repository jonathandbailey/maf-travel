using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Travel.Workflows.Planning.Extensions;

public static class AIContentExtensions
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static DataContent ToDataContent<T>(this FunctionCallContent functionCallContent, string name)
    {
        var argumentsJson = JsonSerializer.Serialize(functionCallContent.Arguments[name]);

        var details = JsonSerializer.Deserialize<T>(argumentsJson, _serializerOptions);

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(details);

        var dataContent = new DataContent(jsonBytes, "application/json");

        return dataContent;
    }

  
}