using Microsoft.Extensions.AI;

namespace Agents.Tools;

public sealed class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, IToolHandler> _handlers;

    public ToolRegistry(IEnumerable<IToolHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.ToolName, StringComparer.OrdinalIgnoreCase);

        var availableTools = _handlers.Values
            .SelectMany(h => h.GetDeclarationOnlyTools())
            .ToList();

        var capabilitiesHandler = new ToolRegistryCapabilitiesHandler(availableTools);
        _handlers[capabilitiesHandler.ToolName] = capabilitiesHandler;
    }

    public IToolHandler? GetHandler(string toolName) =>
        _handlers.TryGetValue(toolName, out var handler) ? handler : null;

    public List<AITool> GetAllDeclarationOnlyTools() =>
        [.. _handlers.Values.SelectMany(h => h.GetDeclarationOnlyTools())];
}
