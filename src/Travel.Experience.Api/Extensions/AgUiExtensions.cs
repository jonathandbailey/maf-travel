using Agents.Services;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Travel.Experience.Application.Agents;

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
  
}