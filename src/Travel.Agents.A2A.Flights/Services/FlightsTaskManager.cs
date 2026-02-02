using A2A;
using Travel.Agents.A2A.Flights.Dto;
using Travel.Agents.A2A.Flights.Extensions;
using Travel.Agents.A2A.Shared.Services;

namespace Travel.Agents.A2A.Flights.Services;

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
        try
        {
            await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.Working, cancellationToken: cancellationToken);

            var dataPart = agentTask.ExtractDataPartWithSkillId();
      
            var dto = A2AExtensions.ToFlightSearchDto(dataPart.Data);

            var response = await _flightService.SearchFlights(dto!, agentTask.ContextId, cancellationToken);

            if (response.Status == FlightAgentStatus.Failed)
            {
                throw new InvalidOperationException(response.Summary);
            }
     
            var message = response.CreateResponseCompleted(agentTask);

            await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.Completed, message, final: true, cancellationToken);
        }
        catch (Exception exception)
        {
            var message = exception.CreateResponseFailed(agentTask);

            await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.Failed, message, final: true, cancellationToken);
        }
    }
}

public interface IFlightsTaskManager
{
    ITaskManager TaskManager { get; }
}