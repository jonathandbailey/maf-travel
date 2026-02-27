using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Travel.Experience.Application.Agents;

namespace Travel.Experience.Api.Extensions;

public static class AgUiExtensions
{
    public static async Task<WebApplication> MapAgUiToAgent(this WebApplication app)
    {
        var conversationAgentFactory = app.Services.GetRequiredService<IConversationAgentFactory>();

        var agUiAgent = await conversationAgentFactory.CreateAsync();

        app.MapAGUI("ag-ui", agUiAgent);

        return app;
    }
  
}