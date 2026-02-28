using Agents.Services;
using Agents.Tools;
using Infrastructure.Repository;
using Microsoft.Agents.AI;

namespace Travel.Experience.Application.Agents;

public interface IConversationAgentFactory
{
    Task<AIAgent> CreateAsync();
}

public class ConversationAgentFactory(
    IAgentFactory agentFactory,
    IToolRegistry registry,
    IAgentTemplateRepository agentTemplateRepository) : IConversationAgentFactory
{
    public async Task<AIAgent> CreateAsync()
    {
        var template = await agentTemplateRepository.LoadAsync("conversation.yaml");

        var agent = await agentFactory.Create(template, registry.GetDeclarationOnlyTools("conversation"));

        var threadAgent = agentFactory.UseMiddleware(agent, "agent-thread");

        var conversationAgent = new ConversationAgent(threadAgent, registry);

        return agentFactory.UseMiddleware(conversationAgent, "agent-ag-ui");
    }
}
