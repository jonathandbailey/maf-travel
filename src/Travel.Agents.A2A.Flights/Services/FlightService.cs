using System.Text.Json;
using System.Text.Json.Serialization;
using Agents.Extensions;
using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Travel.Agents.A2A.Dto;
using Travel.Agents.A2A.Observability;
using Travel.Agents.A2A.Shared.Services;

namespace Travel.Agents.A2A.Flights.Services;

public class FlightService(IAgentFactory agentFactory, IMcpToolsService mcpToolsService) : IFlightService
{
    private const string FlightAgent = "flight_agent";
    private const string AgentThread = "agent-thread";

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public async Task<FlightAgentResponseDto> SearchFlights(FlightSearchDto flightSearchDto, string threadId,
        CancellationToken cancellationToken)
    {
        var flightSchema = AIJsonUtilities.CreateJsonSchema(typeof(FlightAgentResponseDto));

        var flightChatResponseFormat = ChatResponseFormat.ForJsonSchema(
            schema: flightSchema,
            schemaName: "FlightPlan",
            schemaDescription: "User Flight Options for their vacation.");
      
        var mcpTools = await mcpToolsService.ListToolsAsync(cancellationToken: cancellationToken);

        var flightAgent = await agentFactory.Create(FlightAgent, flightChatResponseFormat, tools: [.. mcpTools]);

        agentFactory.UseMiddleware(flightAgent, AgentThread);

        var serialized = JsonSerializer.Serialize(flightSearchDto);

        var requestMessage = $"Flight search : {serialized}";

        using var activity = FlightAgentTelemetry.Start(requestMessage, threadId);

        var agentRunOptions = new ChatClientAgentRunOptions();

        agentRunOptions.AddThreadId(threadId);

        var response = await flightAgent.RunAsync(requestMessage, options:agentRunOptions, cancellationToken: cancellationToken);

        var flightResponse = JsonSerializer.Deserialize<FlightAgentResponseDto>(response.Text, _serializerOptions);

        if (flightResponse == null)
        {
            throw new InvalidOperationException("Failed to deserialize flight agent response.");
        }

        return flightResponse!;
    }
}

public interface IFlightService
{
    Task<FlightAgentResponseDto> SearchFlights(FlightSearchDto flightSearchDto, string threadId,
        CancellationToken cancellationToken);
}