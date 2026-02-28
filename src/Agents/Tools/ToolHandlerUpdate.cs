using Microsoft.Extensions.AI;

namespace Agents.Tools;

public abstract record ToolHandlerUpdate;

public sealed record ToolStatusUpdate(string Message, string? Thought = null, string? Source = null) : ToolHandlerUpdate;

public sealed record ToolResultUpdate(FunctionResultContent Result) : ToolHandlerUpdate;

public sealed record ToolStateSnapshotUpdate(string Type, object Data) : ToolHandlerUpdate;

public record ToolHandlerContext(string ThreadId);
