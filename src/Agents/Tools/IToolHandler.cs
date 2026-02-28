using Microsoft.Extensions.AI;

namespace Agents.Tools;

public interface IToolHandler
{
    string ToolName { get; }

    IAsyncEnumerable<ToolHandlerUpdate> ExecuteAsync(
        FunctionCallContent call,
        ToolHandlerContext context,
        CancellationToken cancellationToken);

    List<AITool> GetDeclarationOnlyTools();
}
