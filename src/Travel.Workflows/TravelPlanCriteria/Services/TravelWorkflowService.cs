using System.Runtime.CompilerServices;
using Infrastructure.Repository;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Workflows.Common;
using Travel.Workflows.Common.Infrastructure;
using Travel.Workflows.Common.Interfaces;
using Travel.Workflows.TravelPlanCriteria.Dto;
using Travel.Workflows.TravelPlanCriteria.Events;

namespace Travel.Workflows.TravelPlanCriteria.Services;

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

        var runRequest = new WorkflowRunRequest(request.Message, request.ThreadId, plan);

        var runner = await CreateRunnerAsync(request.ThreadId);

        try
        {
            await foreach (var evt in runner.WatchStreamAsync(runRequest, cancellationToken))
            {
                if (evt is TravelPlanUpdateEvent planUpdateEvt)
                    await TrySavePlanAsync(planUpdateEvt.TravelPlanState, request.ThreadId, cancellationToken);
                else if (evt is TravelPlanningCompleteEvent completeEvt)
                    await TrySavePlanAsync(completeEvt.TravelPlan, request.ThreadId, cancellationToken);

                yield return evt;
            }
        }
        finally
        {
            await TrySaveSessionAsync(runner, request.ThreadId);
        }
    }

    private async Task<TravelPlanningRunner> CreateRunnerAsync(Guid threadId)
    {
        var session = await sessionRepository.ExistsAsync(threadId)
            ? await sessionRepository.LoadAsync(threadId)
            : new WorkflowSession(threadId, WorkflowState.Created, null);

        var planningTask = agentProvider.CreateAsync(AgentType.Planning);
        var extractingTask = agentProvider.CreateAsync(AgentType.Extracting);

        await Task.WhenAll(planningTask, extractingTask);

        var workflow = TravelWorkflowBuilder.Build(planningTask.Result, extractingTask.Result);

        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore(checkpointRepository, threadId));

        return new TravelPlanningRunner(workflow, checkpointManager, session);
    }

    private async Task TrySavePlanAsync(TravelPlanState planState, Guid threadId, CancellationToken cancellationToken)
    {
        try
        {
            await travelApiClient.UpdatePlanAsync(planState, cancellationToken);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Failed to save travel plan for thread {ThreadId}.", threadId); }
    }

    private async Task TrySaveSessionAsync(TravelPlanningRunner runner, Guid threadId)
    {
        var state = runner.Session.State is WorkflowState.Suspended or WorkflowState.Completed
            ? runner.Session.State
            : WorkflowState.Failed;

        try
        {
            await sessionRepository.SaveAsync(new WorkflowSession(
                threadId,
                state,
                runner.Session.LastCheckpoint));
        }
        catch (Exception ex) { logger.LogWarning(ex, "Failed to save session state for thread {ThreadId}.", threadId); }
    }
}
