using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using Travel.Agents.Shared.Settings;

namespace Travel.Agents.Shared.Services;

public class McpToolsService(HttpClient httpClient, IOptions<ServerSettings> serverSettings) : IMcpToolsService
{
    public async Task<IList<McpClientTool>> ListToolsAsync(CancellationToken cancellationToken)
    {
        var transport = new HttpClientTransport(new()
        {
            Endpoint = new Uri(serverSettings.Value.ServiceUrl),
            Name = "Travel MCP Client",
        }, httpClient);

        var mcpClient = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);

        var mcpTools = await mcpClient.ListToolsAsync(cancellationToken: cancellationToken);
   
        return mcpTools;
    }
}

public interface IMcpToolsService
{
    Task<IList<McpClientTool>> ListToolsAsync(CancellationToken cancellationToken);
}