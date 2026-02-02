using System.Text.Json;
using A2A;
using Travel.Agents.A2A.Flights.Dto;
using Travel.Agents.A2A.Shared.Dto;

namespace Travel.Agents.A2A.Flights.Extensions;

public static class A2AExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static AgentMessage CreateResponseFailed(this Exception exception, AgentTask agentTask)
    {
        var dataPart = new DataPart
        {
            Data = new Dictionary<string, JsonElement>
            {
                ["message"] = JsonSerializer.SerializeToElement(exception.Message),
            }
        };

        var message = new AgentMessage
        {
            Role = MessageRole.Agent,
            ContextId = agentTask.ContextId,
            Parts = [dataPart]
        };


        return message;
    }

    public static AgentMessage CreateResponseCompleted(this FlightAgentResponseDto flightResponse, AgentTask agentTask)
    {
        var dataPart = new DataPart
        {
            Data = new Dictionary<string, JsonElement>
            {
                ["flightSearchId"] = JsonSerializer.SerializeToElement(flightResponse.FlightSearchId),
                ["summary"] = JsonSerializer.SerializeToElement(flightResponse.Summary),
                ["status"] = JsonSerializer.SerializeToElement(flightResponse.Status)
            }
        };

        var message = new AgentMessage
        {
            Role = MessageRole.Agent,
            ContextId = agentTask.ContextId,
            Parts = [dataPart]
        };


        return message;
    }

    public static DataPart CreateDataPartResponse(this FlightAgentResponseDto flightResponse)
    {
        var dataPart = new DataPart
        {
            Data = new Dictionary<string, JsonElement>
            {
                ["flightSearchId"] = JsonSerializer.SerializeToElement(flightResponse.FlightSearchId),
                ["summary"] = JsonSerializer.SerializeToElement(flightResponse.Summary),
                ["status"] = JsonSerializer.SerializeToElement(flightResponse.Status)
            }
        };
        return dataPart;
    }

    public static FlightSearchDto? ToFlightSearchDto(Dictionary<string, JsonElement> data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            return JsonSerializer.Deserialize<FlightSearchDto>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static string ExtractSkillIdFromDataParts(this AgentTask agentTask)
    {
        if (agentTask.History == null)
        {
            throw new InvalidOperationException("AgentTask History is null.");
        }

        if (agentTask.History.Count == 0)
        {
            throw new InvalidOperationException("AgentTask History is empty.");
        }

        foreach (var agentMessage in agentTask.History)
        {
            var parts = agentMessage.Parts.OfType<DataPart>().ToList();

            foreach (var dataPart in parts)
            {
                return dataPart.GetSkillId();
            }
        }

        throw new A2AException("No DataPart with SkillId found in AgentTask History");
    }

    public static DataPart ExtractDataPartWithSkillId(this AgentTask agentTask)
    {
        if (agentTask.History == null)
        {
            throw new InvalidOperationException("AgentTask History is null.");
        }

        if (agentTask.History.Count == 0)
        {
            throw new InvalidOperationException("AgentTask History is empty.");
        }

        foreach (var agentMessage in agentTask.History)
        {
            var parts = agentMessage.Parts.OfType<DataPart>().ToList();

            foreach (var dataPart in parts)
            {
                return dataPart;
            }
        }

        throw new A2AException("No DataPart with SkillId found in AgentTask History");
    }


    public static string GetSkillId(this DataPart dataPart)
    {
        if (dataPart.Metadata == null)
        {
            throw new A2AException("DataPart metadata is null");
        }

        if (dataPart.Metadata.TryGetValue("skill_id", out var skillId))
        {
            if (skillId.ValueKind != JsonValueKind.String)
                throw new A2AException("skill_id in DataPart metadata is not a string");

            var skillValue = skillId.GetString();

            if(skillValue == null)
                throw new A2AException("skill_id in DataPart metadata is null");

            return skillValue;

        }

        throw new A2AException("skill_id not found in DataPart metadata");
    }
}