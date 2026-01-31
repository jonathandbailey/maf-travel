using System.Text.Json;
using A2A;
using Travel.Agents.A2A.Extensions;

namespace Travel.Agents.A2A.Services;

public class FlightsTaskManager : IFlightsTaskManager
{
    private readonly IA2ACardService _cardService;
    private readonly IFlightService _flightService;
    public ITaskManager TaskManager { get; } = new TaskManager();

    public FlightsTaskManager(IA2ACardService cardService, IFlightService flightService)
    {
        _cardService = cardService;
        _flightService = flightService;
    
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
      
        var dto = A2AExtensions.ToFlightSearchDto(dataPart.Data);

        var response = await _flightService.SearchFlights(dto!, agentTask.ContextId, cancellationToken);

        var responsePart = new DataPart
        {
            Data = new Dictionary<string, JsonElement>
            {
                ["flightSearchId"] = JsonSerializer.SerializeToElement(response.FlightSearchId),
                ["summary"] = JsonSerializer.SerializeToElement(response.Summary),
                ["status"] = JsonSerializer.SerializeToElement(response.Status)
            }
        };

        var message = new AgentMessage
        {
            Role = MessageRole.Agent,
            ContextId = agentTask.ContextId,
            Parts = [responsePart]
        };

        await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.Completed, message, final: true, cancellationToken);
    }
}

public interface IFlightsTaskManager
{
    ITaskManager TaskManager { get; }
}