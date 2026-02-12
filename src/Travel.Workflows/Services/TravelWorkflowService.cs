using Infrastructure.Repository;
using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Services;
using Travel.Workflows.Dto;
using Travel.Workflows.Infrastructure;

namespace Travel.Workflows.Services;

public class TravelWorkflowService(
    ITravelPlanService travelPlanService,
    ICheckpointRepository checkpointRepository,
    IAgentProvider agentProvider)
{
    public async IAsyncEnumerable<WorkflowEvent> WatchStreamAsync(TravelWorkflowRequest request)
    {
        var workflowFactory = new WorkflowFactory();

        var planningAgent = await agentProvider.CreateAsync(AgentType.Planning);

        var extractingAgent = await agentProvider.CreateAsync(AgentType.Extracting);

        var workflow = workflowFactory.Build(planningAgent, extractingAgent);

        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore(checkpointRepository, request.ThreadId));

        var travelPlanningWorkflow = new TravelPlanningWorkflow(workflow, checkpointManager);

        var travelPlan = await travelPlanService.GetTravelPlanAsync();

        await foreach (var evt in travelPlanningWorkflow.WatchStreamAsync(request with{ TravelPlan = travelPlan }))
        {
            yield return evt;
        }
    }
}