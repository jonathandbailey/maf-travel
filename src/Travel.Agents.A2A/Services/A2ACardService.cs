using A2A;
using Microsoft.Extensions.Options;
using Travel.Agents.A2A.Settings;

namespace Travel.Agents.A2A.Services;

public class A2ACardService(IOptions<CardSettings> cardSettings) : IA2ACardService
{
    private readonly List<AgentCard> _agentCards = cardSettings.Value.AgentCards;

    public Task<AgentCard> GetAgentCard(string url)
    {
        var path = new Uri(url).AbsolutePath;

        var card = _agentCards.FirstOrDefault(ac => ac.Url == path);

        return Task.FromResult(card ?? throw new A2AException($"Card Not found with Url : {url} "));

    }
}

public interface IA2ACardService
{
    Task<AgentCard> GetAgentCard(string url);
}