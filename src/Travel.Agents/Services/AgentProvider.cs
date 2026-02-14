using Agents.Services;
using Infrastructure.Repository;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Travel.Agents.Services;

public interface IAgentProvider
{
    Task<AIAgent> CreateAsync(AgentType agentType);
    Task<AIAgent> CreateAsync(AgentType agentType, IChatClient chatClient);
}

public class AgentProvider(
    IAgentFactory agentFactory,
    IAgentTemplateRepository agentTemplateRepository) : IAgentProvider
{
    private readonly Dictionary<AgentType, string> _agentTemplates = new()
    {
        { AgentType.Planning, "planning.yaml" },
        { AgentType.Extracting, "extracting.yaml" }
    };

    public async Task<AIAgent> CreateAsync(AgentType agentType)
    {
        var templateName = _agentTemplates.TryGetValue(agentType, out var template)
            ? template
            : throw new ArgumentException($"Unknown agent type: {agentType}", nameof(agentType));

        var agentTemplate = await agentTemplateRepository.LoadAsync(templateName);

        var tools = GetToolsForAgentType(agentType);

        var agent = await agentFactory.Create(agentTemplate, tools);

        return agent;
    }

    public async Task<AIAgent> CreateAsync(AgentType agentType, IChatClient chatClient)
    {
        var templateName = _agentTemplates.TryGetValue(agentType, out var template)
            ? template
            : throw new ArgumentException($"Unknown agent type: {agentType}", nameof(agentType));

        var agentTemplate = await agentTemplateRepository.LoadAsync(templateName);

        var tools = GetToolsForAgentType(agentType);

        var agent = await agentFactory.Create(chatClient, agentTemplate, tools);

        return agent;
    }

    private static List<AITool> GetToolsForAgentType(AgentType agentType) => agentType switch
    {
        AgentType.Planning => PlanningTools.GetDeclarationOnlyTools(),
        AgentType.Extracting => ExtractingTools.GetDeclarationOnlyTools(),
        _ => throw new ArgumentException($"No tools configured for agent type: {agentType}", nameof(agentType))
    };
}
