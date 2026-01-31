using Microsoft.Extensions.AI;
using System.Text.Json;
using Agents.Extensions;
using Agents.Services;
using Microsoft.Agents.AI;
using Travel.Agents.A2A.Dto;
using Travel.Agents.A2A.Observability;

namespace Travel.Agents.A2A.Services;

public class FlightService(IAgentFactory agentFactory) : IFlightService
{
    public async Task<FlightAgentResponseDto> SearchFlights(FlightSearchDto flightSearchDto, string threadId,
        CancellationToken cancellationToken)
    {
        var flightSchema = AIJsonUtilities.CreateJsonSchema(typeof(FlightAgentResponseDto));

        var flightChatResponseFormat = ChatResponseFormat.ForJsonSchema(
            schema: flightSchema,
            schemaName: "FlightPlan",
            schemaDescription: "User Flight Options for their vacation.");
      
     
        var mcpToolService = new McpToolsService(new Uri("http://localhost:5146/"));

        var mcpTools = await mcpToolService.ListToolsAsync(cancellationToken: cancellationToken);

        var flightAgent = await agentFactory.Create("flight_agent", flightChatResponseFormat, tools: [.. mcpTools]);

        agentFactory.UseMiddleware(flightAgent, "agent-thread");

        var serialized = JsonSerializer.Serialize(flightSearchDto);

        var requestMessage = $"Flight search : {serialized}";

        FlightAgentTelemetry.Start(requestMessage, threadId);

        var agentRunOptions = new ChatClientAgentRunOptions();

        agentRunOptions.AddThreadId(threadId);

        var response = await flightAgent.RunAsync(requestMessage, options:agentRunOptions, cancellationToken: cancellationToken);

        var flightResponse = JsonSerializer.Deserialize<FlightAgentResponseDto>(response.Text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true});

        return flightResponse!;
    }
}

public interface IFlightService
{
    Task<FlightAgentResponseDto> SearchFlights(FlightSearchDto flightSearchDto, string threadId,
        CancellationToken cancellationToken);
}