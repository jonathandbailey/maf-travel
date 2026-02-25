using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Travel.Experience.Api.Agents;
using Travel.Experience.Api.Dto;

namespace Travel.Experience.Api.Extensions;

public static class AgUiExtensions
{
    private const string ApplicationJsonMediaType = "application/json";

    public static async Task<WebApplication> MapAgUiToAgent(this WebApplication app)
    {
        var agentFactory = app.Services.GetRequiredService<IAgentFactory>();

        var agent = await agentFactory.Create("conversation_agent");

        var conversationAgent = new ConversationAgent(agent);

      
        var agui =  agentFactory.UseMiddleware(conversationAgent, "agent-thread");
        var thread = agentFactory.UseMiddleware(agui, "agent-ag-ui");

        app.MapAGUI("ag-ui", thread);

        return app;
    }

    public static AgentResponseUpdate ToAgentResponseStatusMessage(this string message)
    {
        var statusUpdate = new StatusUpdate("StatusUpdate", "Conversation Agent", message, string.Empty);

        var snapshot = new SnapShot<StatusUpdate>(statusUpdate.Type, statusUpdate);

        var stateBytes = JsonSerializer.SerializeToUtf8Bytes(snapshot);

        return new AgentResponseUpdate
        {
            Contents = [new DataContent(stateBytes, ApplicationJsonMediaType)]
        };
    }
}