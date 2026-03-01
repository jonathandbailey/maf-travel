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

    public Task<AIAgent> CreateAsync(AgentType agentType) =>
        CreateInternalAsync(agentType, (template, tools) => agentFactory.Create(template, tools));

    public Task<AIAgent> CreateAsync(AgentType agentType, IChatClient chatClient) =>
        CreateInternalAsync(agentType, (template, tools) => agentFactory.Create(chatClient, template, tools));

    private async Task<AIAgent> CreateInternalAsync(AgentType agentType, Func<string, List<AITool>, Task<AIAgent>> factory)
    {
        var templateName = _agentTemplates.TryGetValue(agentType, out var template)
            ? template
            : throw new ArgumentException($"Unknown agent type: {agentType}", nameof(agentType));

        var agentTemplate = await agentTemplateRepository.LoadAsync(templateName);
        var tools = GetToolsForAgentType(agentType);

        return await factory(agentTemplate, tools);
    }

    private static List<AITool> GetToolsForAgentType(AgentType agentType) => agentType switch
    {
        AgentType.Planning => PlanningTools.GetDeclarationOnlyTools(),
        AgentType.Extracting => ExtractingTools.GetDeclarationOnlyTools(),
        _ => throw new ArgumentException($"No tools configured for agent type: {agentType}", nameof(agentType))
    };
}
