using System.Diagnostics;
using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Travel.Workflows.Exceptions;
using Travel.Workflows.Flights.Dto;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Flights.Nodes;

public partial class SearchNode(McpClient mcpClient) : Executor(FlightsNodeNames.SearchNode)
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [MessageHandler(Send = [typeof(FlightSearchResult)])]
    private async ValueTask HandleAsync(FlightSearchCommand command, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(FlightsNodeNames.SearchNode, Guid.Empty);

        var args = new Dictionary<string, object?>
        {
            ["origin"] = command.Origin,
            ["destination"] = command.Destination,
            ["departureDate"] = command.DepartureDate.ToString("yyyy-MM-dd"),
            ["returnDate"] = command.ReturnDate?.ToString("yyyy-MM-dd"),
            ["passengers"] = command.Passengers
        };

        CallToolResult result;
        try
        {
            result = await mcpClient.CallToolAsync("search_flights", args, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw new WorkflowException("SearchNode failed to call search_flights via MCP.", FlightsNodeNames.SearchNode, Guid.Empty, ex);
        }

        if (result.IsError == true)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "MCP search_flights returned an error.");
            throw new WorkflowException("SearchNode: search_flights MCP tool returned an error.", FlightsNodeNames.SearchNode, Guid.Empty);
        }

        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault()
            ?? throw new WorkflowException("SearchNode: search_flights MCP tool returned no text content.", FlightsNodeNames.SearchNode, Guid.Empty);

        var flights = JsonSerializer.Deserialize<List<FlightOption>>(
            textContent.Text,
            s_jsonOptions)
            ?? [];

        activity?.AddNodeAgentOutput($"Found {flights.Count} flight(s).");

        await context.SendMessageAsync(new FlightSearchResult(flights), cancellationToken);
    }
}
