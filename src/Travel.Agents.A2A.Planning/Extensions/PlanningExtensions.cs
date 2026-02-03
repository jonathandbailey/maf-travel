using System.Text.Json;
using A2A;
using Microsoft.Extensions.DependencyInjection;
using Travel.Agents.A2A.Planning.Dto;
using Travel.Agents.A2A.Planning.Services;

namespace Travel.Agents.A2A.Planning.Extensions;

public static class PlanningExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static void AddPlanningServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlanningTaskManager, PlanningTaskManager>();
    }

    public static TravelPlanDto? ExtractTravelPlanSnapshot(this AgentTask agentTask)
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
                if (dataPart.Metadata != null)
                {
                    if (dataPart.Metadata.ContainsKey("name"))
                    {
                        var name = dataPart.Metadata["name"].ToString();

                        if(name == "travel_plan_snapshot")
                        {
                            var json = JsonSerializer.Serialize(dataPart.Data);
                            return JsonSerializer.Deserialize<TravelPlanDto>(json, Options);
                        }
                    }
                }
            }
        }

        throw new A2AException("No DataPart with 'travel_plan_snapshot' found in AgentTask History");
    }

    public static string? ExtractObservation(this AgentTask agentTask)
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
                if (dataPart.Metadata != null)
                {
                    if (dataPart.Metadata.ContainsKey("name"))
                    {
                        var name = dataPart.Metadata["name"].ToString();

                        if (name == "observation")
                        {
                            return JsonSerializer.Serialize(dataPart.Data);
                        }
                    }
                }
            }
        }

        throw new A2AException("No DataPart with 'travel_plan_snapshot' found in AgentTask History");
    }
}