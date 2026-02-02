using A2A;
using Agents.Extensions;
using Microsoft.Extensions.AI;
using System.Text.Json;
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

        var response = await _planningService.RunPlanningAsync(travelPlan!, observation!, agentTask.ContextId, cancellationToken);

        var parts = new List<Part>();

        foreach (var content in response.Messages.SelectMany(m => m.Contents))
        {
            if (content is FunctionCallContent functionCall)
            {
                var argumentsDict = new Dictionary<string, JsonElement>();
                
                if (functionCall.Arguments is IDictionary<string, object?> args)
                {
                    foreach (var kvp in args)
                    {
                        argumentsDict[kvp.Key] = JsonSerializer.SerializeToElement(kvp.Value);
                    }
                }

                var dataPart = new DataPart
                {
                    Data = argumentsDict,
                    Metadata = new Dictionary<string, JsonElement>
                    {
                        ["name"] = JsonSerializer.SerializeToElement(functionCall.Name),
                        ["callId"] = JsonSerializer.SerializeToElement(functionCall.CallId)
                    }
                };
                parts.Add(dataPart);
            }
            else if (content.ToPart() is { } part)
            {
                parts.Add(part);
            }
        }

        var message = new AgentMessage
        {
            Role = MessageRole.Agent,
            ContextId = agentTask.ContextId,
            Parts = parts
        };

        await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.Completed, message, final: true, cancellationToken);
    }
}

public interface IPlanningTaskManager
{
    ITaskManager TaskManager { get; }
}