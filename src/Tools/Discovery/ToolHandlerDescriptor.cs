namespace Tools.Discovery;

internal sealed record ToolHandlerDescriptor(Type HandlerType, IReadOnlyList<string> Groups);
