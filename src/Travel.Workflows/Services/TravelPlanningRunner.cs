using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Exceptions;

namespace Travel.Workflows.Services;

public class TravelPlanningRunner(Workflow workflow, CheckpointManager checkpointManager, WorkflowSession session)
{
    private CheckpointInfo? _checkpointInfo = session.LastCheckpoint;
    private WorkflowState _state = session.State;

    public WorkflowState State => _state;
    public CheckpointInfo? LastCheckpoint => _checkpointInfo;

    public WorkflowSession Session => session;

    public async IAsyncEnumerable<WorkflowEvent> WatchStreamAsync(TravelWorkflowRequest request)
    {
        var run = await CreateWorkflowRun(request);

        await foreach (var evt in run.WatchStreamAsync())
        {
            switch (evt)
            {
                case SuperStepCompletedEvent superStepCompletedEvt:
                    HandleSuperStepCompletedEvent(superStepCompletedEvt);
                    break;
                case RequestInfoEvent requestInfoEvent:
                    switch (_state)
                    {
                        case WorkflowState.Executing:
                        {
                            _state = WorkflowState.Suspended;

                            yield return evt;
                            yield break;
                        }
                        case WorkflowState.Suspended:
                        {
                            var resp = requestInfoEvent.Request.CreateResponse(new InformationResponse(request.Message));

                            _state = WorkflowState.Executing;
                            await run.SendResponseAsync(resp);
                            break;
                        }
                    }

                    break;
                case TravelPlanStatusUpdateEvent:
                case TravelPlanUpdateEvent:
                    yield return evt;
                    break;
                case TravelPlanningCompleteEvent:
                    _state = WorkflowState.Completed;

                    yield return evt;
                    break;
            }
        }
    }

   

    private void HandleSuperStepCompletedEvent(SuperStepCompletedEvent superStepCompletedEvt)
    {
        var checkpoint = superStepCompletedEvt.CompletionInfo!.Checkpoint;

        if (checkpoint != null)
        {
            _checkpointInfo = checkpoint;
        }
    }


    private async ValueTask<StreamingRun> CreateWorkflowRun(TravelWorkflowRequest request)
    {
        return _state switch
        {
            WorkflowState.Failed => throw new WorkflowException("Workflow cannot be started or resumed while in an Failed state."),
            WorkflowState.Executing => throw new WorkflowException("Workflow cannot be started or resumed while in an Executing state."),
            WorkflowState.Suspended => await ResumeWorkflow(),
            WorkflowState.Created => await RunWorkflow(request),
            WorkflowState.Completed => throw new WorkflowException("Workflow cannot be started or resumed while in an Executing state."),
            _ => throw new WorkflowException("Invalid workflow state.")
        };
    }

    private async ValueTask<StreamingRun> RunWorkflow(TravelWorkflowRequest request)
    {
        _state = WorkflowState.Executing;

        var run = await InProcessExecution.RunStreamingAsync(workflow, request, checkpointManager);
        return run;
    }

    private async ValueTask<StreamingRun> ResumeWorkflow()
    {
        if (_checkpointInfo == null)
        {
            throw new WorkflowException("CheckpointInfo is NULL, but checkpoint information is required to resume a workflow.");
        }

        var run = await InProcessExecution.ResumeStreamingAsync(workflow, _checkpointInfo,
            checkpointManager);

        _state = WorkflowState.Suspended;

        return run;
    }
}