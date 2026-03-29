using Microsoft.Extensions.AI;

namespace Tools.Registry;

public sealed class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, IToolHandler> _handlers;
    private readonly Dictionary<string, List<IToolHandler>> _groups;

    public ToolRegistry(IEnumerable<IToolHandler> handlers)
        : this(handlers.Select(h => new ToolHandlerRegistration(h, [])))
    {
    }

    public ToolRegistry(IEnumerable<ToolHandlerRegistration> registrations)
    {
        _handlers = new(StringComparer.OrdinalIgnoreCase);
        _groups = new(StringComparer.OrdinalIgnoreCase);

        foreach (var reg in registrations)
        {
            _handlers[reg.Handler.ToolName] = reg.Handler;

            foreach (var group in reg.Groups)
            {
                if (!_groups.TryGetValue(group, out var list))
                {
                    list = [];
                    _groups[group] = list;
                }
                list.Add(reg.Handler);
            }
        }

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

    public List<AITool> GetDeclarationOnlyTools(string group) =>
        _groups.TryGetValue(group, out var handlers)
            ? [.. handlers.SelectMany(h => h.GetDeclarationOnlyTools())]
            : [];
}
