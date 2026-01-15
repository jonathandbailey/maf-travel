using A2A;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.A2A;
using Microsoft.Extensions.AI;

namespace Application.Services;

public class A2AAgentServiceDiscovery : IA2AAgentServiceDiscovery
{
    private const string _endppoint = "https://localhost:7027";

    private readonly List<AgentToolSettings> _agentToolSettings =
    [
        new AgentToolSettings()
        {
         
            CardPath = "/api/a2a/travel/v1/card"
        }
    ];

    private readonly List<AgentMeta> _agentMetas = [];

    public List<AITool> GetTools()
    {
        return _agentMetas.Select(x => x.Tool).ToList();
    }

    public async Task Initialize()
    {
        foreach (var agentToolSetting in _agentToolSettings)
        {
            var cardResolver = new A2ACardResolver(new Uri(_endppoint), new HttpClient(), agentCardPath: agentToolSetting.CardPath);

            var card = await cardResolver.GetAgentCardAsync();

            var client = new A2AClient(new Uri(card.Url), new HttpClient());

            var agent = new A2AAgent(client, name: card.Name, description: card.Description);

            var tool = agent.AsAIFunction();

            var agentMeta = new AgentMeta(card.Name, agent, card, tool);

            _agentMetas.Add(agentMeta);
        }
    }
}

public class AgentToolSettings
{
    public required string CardPath { get; init; }
}

public class AgentMeta(string name, A2AAgent agent, AgentCard card, AITool tool)
{
    public string Name { get; } = name;

    public A2AAgent Agent { get; } = agent;

    public AgentCard Card { get; } = card;

    public AITool Tool { get; } = tool;
}

public interface IA2AAgentServiceDiscovery
{
    Task Initialize();
    List<AITool> GetTools();
}