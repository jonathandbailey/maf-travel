namespace Agents.Tools;

public record ToolHandlerRegistration(IToolHandler Handler, IReadOnlyList<string> Groups);
