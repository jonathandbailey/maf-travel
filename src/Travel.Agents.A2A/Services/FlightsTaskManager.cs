using A2A;

namespace Travel.Agents.A2A.Services;



public class FlightsTaskManager : IFlightsTaskManager
{
    private readonly IA2ACardService _cardService;
    public ITaskManager TaskManager { get; } = new TaskManager();

    public FlightsTaskManager(IA2ACardService cardService)
    {
        _cardService = cardService;
        TaskManager.OnTaskCreated += OnTaskCreated;
        TaskManager.OnAgentCardQuery += OnAgentCardQuery;
    }

    private Task<AgentCard> OnAgentCardQuery(string url, CancellationToken cancellationToken)
    {
        return _cardService.GetAgentCard(url);
    }

    private async Task OnTaskCreated(AgentTask agentTask, CancellationToken cancellationToken)
    {


        var message = new AgentMessage
        {
            Role = MessageRole.Agent,
            ContextId = agentTask.ContextId,
            Parts = [new TextPart() { Text = "Flight Task Complete." }]
        };

        await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.Completed, message, final: true, cancellationToken);
    }
}

public interface IFlightsTaskManager
{
    ITaskManager TaskManager { get; }
}