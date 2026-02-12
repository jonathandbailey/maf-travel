using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Services;
using Travel.Workflows.Dto;

namespace Travel.Workflows.Services;

public class TravelWorkflowService(
    ITravelPlanService travelPlanService,
    IAgentProvider agentProvider)
{
    public async IAsyncEnumerable<WorkflowEvent> WatchStreamAsync(TravelWorkflowRequest request)
    {
        var workflowFactory = new WorkflowFactory();

        var planningAgent = await agentProvider.CreateAsync(AgentType.Planning);

        var extractingAgent = await agentProvider.CreateAsync(AgentType.Extracting);

        var workflow = workflowFactory.Build(planningAgent, extractingAgent);

        var checkpointManager = CheckpointManager.Default;

        var travelPlanningWorkflow = new TravelPlanningWorkflow(workflow, checkpointManager);

        var travelPlan = await travelPlanService.GetTravelPlanAsync();

        await foreach (var evt in travelPlanningWorkflow.WatchStreamAsync(request with{ TravelPlan = travelPlan }))
        {
            yield return evt;
        }
    }
}