using System.Runtime.CompilerServices;
using Infrastructure.Repository;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Travel.Agents.Services;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Infrastructure;
using Travel.Workflows.Interfaces;

namespace Travel.Workflows.Services;

public class TravelWorkflowService(
    ICheckpointRepository checkpointRepository,
    IWorkflowSessionRepository sessionRepository,
    IAgentProvider agentProvider,
    ITravelApiClient travelApiClient,
    ILogger<TravelWorkflowService> logger)
{
    public async IAsyncEnumerable<WorkflowEvent> WatchStreamAsync(TravelWorkflowRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var plan = await travelApiClient.GetPlanBySessionAsync(request.ThreadId, cancellationToken);

        var id = plan.Id;

        var isNewSession = !await sessionRepository.ExistsAsync(request.ThreadId);
        if (isNewSession)
            request = request with { TravelPlan = plan };

        var runner = await CreateRunnerAsync(request);

        try
        {
            await foreach (var evt in runner.WatchStreamAsync(request, cancellationToken))
            {
                var planState = evt switch
                {
                    TravelPlanUpdateEvent e => e.TravelPlanState,
                    TravelPlanningCompleteEvent e => e.TravelPlan,
                    _ => null
                };

                if (planState is not null)
                {
                    planState.Id = id;
                    try
                    {
                        await travelApiClient.UpdatePlanAsync(planState, cancellationToken);
                    }
                    catch (Exception ex) { logger.LogWarning(ex, "Failed to save travel plan for thread {ThreadId}.", request.ThreadId); }
                }

                yield return evt;
            }
        }
        finally
        {
            var state = runner.Session.State is WorkflowState.Suspended or WorkflowState.Completed
                ? runner.Session.State
                : WorkflowState.Failed;

            try
            {
                await sessionRepository.SaveAsync(new WorkflowSession(
                    request.ThreadId,
                    state,
                    runner.Session.LastCheckpoint));
            }
            catch (Exception ex) { logger.LogWarning(ex, "Failed to save session state for thread {ThreadId}.", request.ThreadId); }
        }
    }

    private async Task<TravelPlanningRunner> CreateRunnerAsync(TravelWorkflowRequest request)
    {
        var loaded = await sessionRepository.ExistsAsync(request.ThreadId)
            ? await sessionRepository.LoadAsync(request.ThreadId)
            : null;
        var session = loaded ?? new WorkflowSession(request.ThreadId, WorkflowState.Created, null);

        var planningTask = agentProvider.CreateAsync(AgentType.Planning);
        var extractingTask = agentProvider.CreateAsync(AgentType.Extracting);
        await Task.WhenAll(planningTask, extractingTask);

        var workflow = TravelWorkflowBuilder.Build(planningTask.Result, extractingTask.Result);

        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore(checkpointRepository, request.ThreadId));

        return new TravelPlanningRunner(workflow, checkpointManager, session);
    }
}
