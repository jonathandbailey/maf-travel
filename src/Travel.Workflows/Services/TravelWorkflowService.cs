using Infrastructure.Repository;
using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Services;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Infrastructure;

namespace Travel.Workflows.Services;

public class TravelWorkflowService(
    ITravelPlanService travelPlanService,
    ICheckpointRepository checkpointRepository,
    IWorkflowSessionRepository sessionRepository,
    IAgentProvider agentProvider)
{
    public async IAsyncEnumerable<WorkflowEvent> WatchStreamAsync(TravelWorkflowRequest request)
    {
        var session = await sessionRepository.LoadAsync(request.ThreadId);

        var workflowFactory = new WorkflowFactory();

        var planningAgent = await agentProvider.CreateAsync(AgentType.Planning);

        var extractingAgent = await agentProvider.CreateAsync(AgentType.Extracting);

        var workflow = workflowFactory.Build(planningAgent, extractingAgent);

        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore(checkpointRepository, request.ThreadId));

        var travelPlanningWorkflow = new TravelPlanningRunner(workflow, checkpointManager, session);

        var travelPlan = await travelPlanService.GetTravelPlanAsync();

        try
        {
            await foreach (var evt in travelPlanningWorkflow.WatchStreamAsync(request with { TravelPlan = travelPlan }))
            {
                yield return evt;
            }
        }
        finally
        {
            await sessionRepository.SaveAsync(new WorkflowSession(
                request.ThreadId,
                travelPlanningWorkflow.State,
                travelPlanningWorkflow.LastCheckpoint));
        }
    }
}