using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Travel.Experience.Application.Agents.ToolHandling;

public sealed class ToolRegistryCapabilitiesHandler(IReadOnlyList<AITool> availableTools) : IToolHandler
{
    public const string CapabilitiesToolName = "get_application_capabilities";

    [Description("Returns a detailed list of all available tools and their capabilities in this application. Use this to inform the user about what the application can do.")]
    private static string GetApplicationCapabilities() => "";

    private static readonly AIFunction CapabilitiesFunction =
        AIFunctionFactory.Create(GetApplicationCapabilities, CapabilitiesToolName);

    public string ToolName => CapabilitiesToolName;

    public List<AITool> GetDeclarationOnlyTools() =>
        [CapabilitiesFunction.AsDeclarationOnly()];

    public async IAsyncEnumerable<ToolHandlerUpdate> ExecuteAsync(
        FunctionCallContent call,
        ToolHandlerContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var description = BuildCapabilitiesDescription(availableTools);
        yield return new ToolResultUpdate(new FunctionResultContent(call.CallId, description));
    }

    private static string BuildCapabilitiesDescription(IReadOnlyList<AITool> tools)
    {
        var sb = new StringBuilder();
        sb.AppendLine("The following capabilities are available in this application:");
        sb.AppendLine();

        foreach (var tool in tools)
        {
            sb.AppendLine($"### {tool.Name}");

            if (!string.IsNullOrWhiteSpace(tool.Description))
                sb.AppendLine(tool.Description);

            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }
}
