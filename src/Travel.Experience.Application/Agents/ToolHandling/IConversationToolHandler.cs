using Microsoft.Extensions.AI;

namespace Travel.Experience.Application.Agents.ToolHandling;

public interface IConversationToolHandler
{
    string ToolName { get; }

    IAsyncEnumerable<ToolHandlerUpdate> ExecuteAsync(
        FunctionCallContent call,
        ToolHandlerContext context,
        CancellationToken cancellationToken);
}
