using Infrastructure.Repository;
using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Services;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Infrastructure;

namespace Travel.Workflows.Services;

public class TravelWorkflowService(
    ICheckpointRepository checkpointRepository,
    IWorkflowSessionRepository sessionRepository,
    IAgentProvider agentProvider)
{
    public async IAsyncEnumerable<WorkflowEvent> WatchStreamAsync(TravelWorkflowRequest request)
    {
        var runner = await CreateRunnerAsync(request);

        try
        {
            await foreach (var evt in runner.WatchStreamAsync(request))
            {
                yield return evt;
            }
        }
        finally
        {
            var state = runner.State is WorkflowState.Suspended or WorkflowState.Completed
                ? runner.State
                : WorkflowState.Failed;

            try
            {
                await sessionRepository.SaveAsync(new WorkflowSession(
                    request.ThreadId,
                    state,
                    runner.LastCheckpoint));
            }
            catch { /* swallow to avoid masking a stream exception */ }
        }
    }

    private async Task<TravelPlanningRunner> CreateRunnerAsync(TravelWorkflowRequest request)
    {
        var session = await sessionRepository.LoadAsync(request.ThreadId);

        var planningTask = agentProvider.CreateAsync(AgentType.Planning);
        var extractingTask = agentProvider.CreateAsync(AgentType.Extracting);
        await Task.WhenAll(planningTask, extractingTask);

        var workflow = WorkflowFactory.Build(planningTask.Result, extractingTask.Result);

        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore(checkpointRepository, request.ThreadId));

        return new TravelPlanningRunner(workflow, checkpointManager, session);
    }
}
