using Microsoft.Extensions.AI;

namespace Tools.Registry;

public interface IToolHandler
{
    string ToolName { get; }

    IAsyncEnumerable<ToolHandlerUpdate> ExecuteAsync(
        FunctionCallContent call,
        ToolHandlerContext context,
        CancellationToken cancellationToken);

    List<AITool> GetDeclarationOnlyTools();
}
