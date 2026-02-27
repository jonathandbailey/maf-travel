using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Travel.Experience.Application.Dto;

namespace Travel.Experience.Application.Extensions;

public static class AgUiExtensions
{
    private const string ApplicationJsonMediaType = "application/json";

    public static AgentResponseUpdate ToAgentResponseStatusMessage(this string message, string? thought = null, string? source = null)
    {
        var statusUpdate = new StatusUpdate("StatusUpdate", source ?? "Conversation Agent", message, thought ?? string.Empty);

        var snapshot = new SnapShot<StatusUpdate>(statusUpdate.Type, statusUpdate);

        var stateBytes = JsonSerializer.SerializeToUtf8Bytes(snapshot);

        return new AgentResponseUpdate
        {
            Contents = [new DataContent(stateBytes, ApplicationJsonMediaType)]
        };
    }
}