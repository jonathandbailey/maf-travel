using A2A;
using Travel.Agents.A2A.Planning.Extensions;
using Travel.Agents.A2A.Shared.Services;

namespace Travel.Agents.A2A.Planning.Services;

public class PlanningTaskManager : IPlanningTaskManager
{
    private readonly IA2ACardService _cardService;
    private readonly IPlanningService _planningService;
    public ITaskManager TaskManager { get; } = new TaskManager();

    public PlanningTaskManager(IA2ACardService cardService, IPlanningService planningService)
    {
        _cardService = cardService;
        _planningService = planningService;

        TaskManager.OnTaskCreated += OnTaskCreated;
        TaskManager.OnAgentCardQuery += OnAgentCardQuery;
    }

    private Task<AgentCard> OnAgentCardQuery(string url, CancellationToken cancellationToken)
    {
        return _cardService.GetAgentCard(url);
    }

    private async Task OnTaskCreated(AgentTask agentTask, CancellationToken cancellationToken)
    {
        var travelPlan = agentTask.ExtractTravelPlanSnapshot();

        var observation = agentTask.ExtractObservation();

        await _planningService.RunPlanningAsync(travelPlan!, observation!, agentTask.ContextId, cancellationToken);

        var message = new AgentMessage
        {
            Role = MessageRole.Agent,
            ContextId = agentTask.ContextId,
        };

        await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.Completed, message, final: true, cancellationToken);
    }
}

public interface IPlanningTaskManager
{
    ITaskManager TaskManager { get; }
}