using ModelContextProtocol.Client;
using System.Threading;

namespace Travel.Agents.A2A.Services;

public class McpToolsService(Uri serverUri)
{
    public async Task<IList<McpClientTool>> ListToolsAsync(CancellationToken cancellationToken)
    {
        var httpClient = new HttpClient();

        var transport = new HttpClientTransport(new()
        {
            Endpoint = serverUri,
            Name = "Travel MCP Client",
        }, httpClient);

        var mcpClient = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);

        var mcpTools = await mcpClient.ListToolsAsync(cancellationToken: cancellationToken);
   
        return mcpTools;
    }
}