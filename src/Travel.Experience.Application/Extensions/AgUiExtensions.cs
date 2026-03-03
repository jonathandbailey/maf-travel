using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Travel.Experience.Application.Dto;

namespace Travel.Experience.Application.Extensions;

public static class AgUiExtensions
{
    private const string ApplicationJsonMediaType = "application/json";

    public static AgentResponseUpdate ToAgentResponseStateSnapshot(this object data, string type)
    {
        var snapshot = new SnapShot<object>(type, data);
        var stateBytes = JsonSerializer.SerializeToUtf8Bytes(snapshot);
        return new AgentResponseUpdate
        {
            Contents = [new DataContent(stateBytes, ApplicationJsonMediaType)]
        };
    }

    public static AgentResponseUpdate ToAgentResponseStatusMessage(this string message, string? thought = null, string? source = null)
    {
        var statusUpdate = new StatusUpdate("StatusUpdate", source ?? "Conversation", message, thought ?? string.Empty);

        var snapshot = new SnapShot<StatusUpdate>(statusUpdate.Type, statusUpdate);

        var stateBytes = JsonSerializer.SerializeToUtf8Bytes(snapshot);

        return new AgentResponseUpdate
        {
            Contents = [new DataContent(stateBytes, ApplicationJsonMediaType)]
        };
    }

    public static AgentResponseUpdate ToAgentResponseRunError(this string message)
    {
        var errorUpdate = new StatusUpdate("RunError", "TravelWorkflow", message, string.Empty);
        var snapshot = new SnapShot<StatusUpdate>("RunError", errorUpdate);
        var stateBytes = JsonSerializer.SerializeToUtf8Bytes(snapshot);
        return new AgentResponseUpdate
        {
            Contents = [new DataContent(stateBytes, ApplicationJsonMediaType)]
        };
    }
}