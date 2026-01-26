using System.Text.Json;
using Infrastructure.Interfaces;

namespace Agents.Services;

public class AgentMemoryService(IArtifactRepository artifactRepository) : IAgentMemoryService
{
    private const string Container = "agents";

    public async Task SaveAsync(AgentState state, string name)
    {
        await artifactRepository.SaveAsync(state, name, Container);
    }

    public async Task<bool> ExistsAsync(string name)
    {
        return await artifactRepository.ExistsAsync(name, Container);
    }

    public async Task<AgentState> LoadAsync(string name)
    {
        var agentState = await artifactRepository.LoadAsync<AgentState>(name, Container);

        return agentState;
    }
}

public interface IAgentMemoryService
{
    Task SaveAsync(AgentState state, string name);
    Task<bool> ExistsAsync(string name);
    Task<AgentState> LoadAsync(string name);
}

public class AgentState(JsonElement thread)
{
    public JsonElement Thread { get; } = thread;
}