using System.Text.Json;
using System.Text.Json.Serialization;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Tests.Common;
using TravelPlanDto = Travel.Workflows.Dto.TravelPlanDto;

namespace Travel.Tests.Helper;

public class AgentCreateMetaJsonConverter : JsonConverter<AgentFactoryHelper.AgentCreateMeta>
{
    public override AgentFactoryHelper.AgentCreateMeta Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var agentTypeString = root.GetProperty("AgentType").GetString();
        var agentType = Enum.Parse<AgentType>(agentTypeString!);
        var name = root.GetProperty("Name").GetString()!;

        string? argumentsKey = null;
        if (root.TryGetProperty("ArgumentsKey", out var argumentsKeyElement))
        {
            argumentsKey = argumentsKeyElement.GetString();
        }

        object? arguments = null;
        if (root.TryGetProperty("Arguments", out var argumentsElement))
        {
            // Deserialize based on the tool name to get the correct type
            if (name == ExtractingTools.UpdateTravelPlanToolName)
            {
                arguments = JsonSerializer.Deserialize<TravelPlanDto>(argumentsElement.GetRawText(), options);
            }
            else if (name == PlanningTools.RequestInformationToolName)
            {
                arguments = JsonSerializer.Deserialize<RequestInformationDto>(argumentsElement.GetRawText(), options);
            }
            else
            {
                // Default to JsonElement for unknown types
                arguments = argumentsElement.Clone();
            }
        }

        return new AgentFactoryHelper.AgentCreateMeta(agentType, name, argumentsKey, arguments);
    }

    public override void Write(Utf8JsonWriter writer, AgentFactoryHelper.AgentCreateMeta value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("AgentType", value.AgentType.ToString());
        writer.WriteString("Name", value.Name);

        if (value.ArgumentsKey != null)
        {
            writer.WriteString("ArgumentsKey", value.ArgumentsKey);
        }

        if (value.Arguments != null)
        {
            writer.WritePropertyName("Arguments");
            JsonSerializer.Serialize(writer, value.Arguments, options);
        }

        writer.WriteEndObject();
    }
}
