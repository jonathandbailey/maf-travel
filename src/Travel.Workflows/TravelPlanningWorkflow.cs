using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Dto;

namespace Travel.Workflows;

public class TravelPlanningWorkflow(Workflow workflow, CheckpointManager checkpointManager)
{
    private CheckpointInfo? _checkpointInfo;
    private WorkflowState _state = WorkflowState.Created;

    public async IAsyncEnumerable<WorkflowEvent> WatchStreamAsync(
        TravelWorkflowRequest request)
    {
        _checkpointInfo = request.CheckpointInfo;
        var run = await CreateWorkflowRun(request);

        await foreach (var evt in run.WatchStreamAsync())
        {
            if (evt is SuperStepCompletedEvent superStepCompletedEvt)
            {
                var checkpoint = superStepCompletedEvt.CompletionInfo!.Checkpoint;

                if (checkpoint != null)
                {
                    _checkpointInfo = checkpoint;
                }

                yield return evt;
            }

            if (evt is RequestInfoEvent requestInfoEvent)
            {
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
            }

            yield return evt;
        }
    }

    private async ValueTask<StreamingRun> CreateWorkflowRun(TravelWorkflowRequest request)
    {
        if (_checkpointInfo != null)
        {
            _state = WorkflowState.Suspended;

            return await ResumeWorkflow();
        }

        if (_state == WorkflowState.Created)
        {
            _state = WorkflowState.Executing;
        }

        return await InProcessExecution.RunStreamingAsync(workflow, request, checkpointManager);

        return _state switch
        {
            WorkflowState.Failed => throw new WorkflowException("Workflow cannot be started or resumed while in an Failed state."),
            WorkflowState.Executing => throw new WorkflowException("Workflow cannot be started or resumed while in an Executing state."),
            WorkflowState.Suspended => await ResumeWorkflow(),
            WorkflowState.Created => await InProcessExecution.RunStreamingAsync(workflow, request.Message, checkpointManager),
            WorkflowState.Completed => throw new WorkflowException("Workflow cannot be started or resumed while in an Executing state."),
            _ => throw new WorkflowException("Invalid workflow state.")
        };
    }

    private async ValueTask<StreamingRun> ResumeWorkflow()
    {
        if (_checkpointInfo == null)
        {
            throw new WorkflowException("CheckpointInfo is NULL, but checkpoint information is required to resume a workflow.");
        }
        
        return await InProcessExecution.ResumeStreamingAsync(workflow, _checkpointInfo,
            checkpointManager);
    }
}