using System.Text.Json;
using A2A;
using Agents.Services;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Travel.Agents.A2A.Dto;
using Travel.Agents.A2A.Extensions;


namespace Travel.Agents.A2A.Services;

public class FlightsTaskManager : IFlightsTaskManager
{
    private readonly IA2ACardService _cardService;
    private readonly IAgentFactory _agentFactory;
    public ITaskManager TaskManager { get; } = new TaskManager();

    public FlightsTaskManager(IA2ACardService cardService, IAgentFactory agentFactory)
    {
        _cardService = cardService;
        _agentFactory = agentFactory;
        TaskManager.OnTaskCreated += OnTaskCreated;
        TaskManager.OnAgentCardQuery += OnAgentCardQuery;
    }

    private Task<AgentCard> OnAgentCardQuery(string url, CancellationToken cancellationToken)
    {
        return _cardService.GetAgentCard(url);
    }

    private async Task OnTaskCreated(AgentTask agentTask, CancellationToken cancellationToken)
    {
        var dataPart = agentTask.ExtractDataPartWithSkillId();

        var id = dataPart.GetSkillId();

        var dto = A2AExtensions.ToFlightSearchDto(dataPart.Data);

        var fllightSchema = AIJsonUtilities.CreateJsonSchema(typeof(FlightAgentResponseDto));

        var flightChatResponseFormat = ChatResponseFormat.ForJsonSchema(
            schema: fllightSchema,
            schemaName: "FlightPlan",
            schemaDescription: "User Flight Options for their vacation.");

        var httpClient = new HttpClient();

        var serverUrl = "http://localhost:5146/";
        var transport = new HttpClientTransport(new()
        {
            Endpoint = new Uri(serverUrl),
            Name = "Travel MCP Client",
        }, httpClient);

        var mcpClient = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);

        var mcpTools = await mcpClient.ListToolsAsync(cancellationToken:cancellationToken);

        var flightAgent = await _agentFactory.Create("flight_agent", flightChatResponseFormat, tools: [.. mcpTools]);

        //_agentFactory.UseMiddleware(flightAgent, "agent-thread");

        var serialized = JsonSerializer.Serialize(dto);

        var requestMessage = $"Flight search : {serialized}";

        var response = await flightAgent.RunAsync(requestMessage, cancellationToken: cancellationToken);

        var message = new AgentMessage
        {
            Role = MessageRole.Agent,
            ContextId = agentTask.ContextId,
            Parts = [new TextPart { Text = response.Text }]
        };

        await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.Completed, message, final: true, cancellationToken);
    }
}

public interface IFlightsTaskManager
{
    ITaskManager TaskManager { get; }
}