using System.Text.Json;

namespace Agents.Services;

public class AgentState(JsonElement thread)
{
    public JsonElement Thread { get; } = thread;
}