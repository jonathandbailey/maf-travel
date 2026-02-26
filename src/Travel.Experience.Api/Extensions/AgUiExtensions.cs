using Agents.Services;
using Infrastructure.Repository;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Travel.Experience.Application.Agents;
using Travel.Experience.Application.Agents.ToolHandling;

namespace Travel.Experience.Api.Extensions;

public static class AgUiExtensions
{
    public static async Task<WebApplication> MapAgUiToAgent(this WebApplication app)
    {
        var agentFactory = app.Services.GetRequiredService<IAgentFactory>();

        var registry = app.Services.GetRequiredService<IConversationToolHandlerRegistry>();

        var agentTemplateRepository = app.Services.GetRequiredService<IAgentTemplateRepository>();

        var template = await agentTemplateRepository.LoadAsync("conversation.yaml");

        var agent = await agentFactory.Create(template, ConversationAgentTools.GetDeclarationOnlyTools());

        var threadAgent = agentFactory.UseMiddleware(agent, "agent-thread");

        var conversationAgent = new ConversationAgent(threadAgent, registry);
        
        var agUiAgent = agentFactory.UseMiddleware(conversationAgent, "agent-ag-ui");

        app.MapAGUI("ag-ui", agUiAgent);

        return app;
    }
  
}