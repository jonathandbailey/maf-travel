using Microsoft.Extensions.AI;

namespace Tools.Registry;

public abstract record ToolHandlerUpdate;

public sealed record ToolStatusUpdate(string Message, string? Thought = null, string? Source = null) : ToolHandlerUpdate;

public sealed record ToolResultUpdate(FunctionResultContent Result) : ToolHandlerUpdate;

public sealed record ToolStateSnapshotUpdate(string Type, object Data) : ToolHandlerUpdate;

public sealed record ToolErrorUpdate(string Message) : ToolHandlerUpdate;

public record ToolHandlerContext(Guid ThreadId);
