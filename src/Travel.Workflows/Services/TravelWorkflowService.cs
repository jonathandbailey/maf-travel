using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Agents.Services;
using Travel.Workflows.Dto;

namespace Travel.Workflows.Services;

public class TravelWorkflowService(
    IAgentProvider agentProvider, 
    ITravelPlanService travelPlanService)
{
    public async IAsyncEnumerable<WorkflowEvent> WatchStreamAsync(TravelWorkflowRequest request)
    {
        var workflowFactory = new WorkflowFactory(travelPlanService);

        var planningAgent = await agentProvider.CreateAsync(AgentType.Planning);

        var extractingAgent = await agentProvider.CreateAsync(AgentType.Extracting);

        var workflow = workflowFactory.Build(planningAgent, extractingAgent);

        var checkpointManager = CheckpointManager.Default;

        var travelPlanningWorkflow = new TravelPlanningWorkflow();

        var message = new ChatMessage(ChatRole.User, request.Message);

        await foreach (var evt in travelPlanningWorkflow.WatchStreamAsync(workflow, request.CheckpointInfo, checkpointManager, message))
        {
            yield return evt;
        }
    }
}