using Microsoft.Extensions.AI;

namespace Travel.Experience.Application.Agents.ToolHandling;

public sealed class ToolRegistry(IEnumerable<IToolHandler> handlers)
    : IToolRegistry
{
    private readonly Dictionary<string, IToolHandler> _handlers =
        handlers.ToDictionary(h => h.ToolName, StringComparer.OrdinalIgnoreCase);

    public IToolHandler? GetHandler(string toolName) =>
        _handlers.TryGetValue(toolName, out var handler) ? handler : null;

    public List<AITool> GetAllDeclarationOnlyTools() =>
        [.. _handlers.Values.SelectMany(h => h.GetDeclarationOnlyTools())];
}
