namespace Travel.Experience.Application.Agents.ToolHandling;

public sealed class ConversationToolHandlerRegistry(IEnumerable<IConversationToolHandler> handlers)
    : IConversationToolHandlerRegistry
{
    private readonly Dictionary<string, IConversationToolHandler> _handlers =
        handlers.ToDictionary(h => h.ToolName, StringComparer.OrdinalIgnoreCase);

    public IConversationToolHandler? GetHandler(string toolName) =>
        _handlers.TryGetValue(toolName, out var handler) ? handler : null;
}
