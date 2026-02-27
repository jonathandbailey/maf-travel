using Microsoft.Extensions.AI;

namespace Travel.Experience.Application.Agents.ToolHandling;

public abstract record ToolHandlerUpdate;

public sealed record ToolStatusUpdate(string Message, string? Thought = null, string? Source = null) : ToolHandlerUpdate;

public sealed record ToolResultUpdate(FunctionResultContent Result) : ToolHandlerUpdate;

public sealed record ToolStateSnapshotUpdate(string Type, object Data) : ToolHandlerUpdate;

public record ToolHandlerContext(string ThreadId);
