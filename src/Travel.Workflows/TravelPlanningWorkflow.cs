using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
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
        var run = await CreateWorkflowRun(request.Message);

        if (_state == WorkflowState.Created)
        {
            _state = WorkflowState.Executing;
        }

        await foreach (var evt in run.Run.WatchStreamAsync())
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
                        await run.Run.SendResponseAsync(resp);
                        break;   
                    }
                }
            }

            yield return evt;
        }
    }

    private async Task<Checkpointed<StreamingRun>> CreateWorkflowRun(ChatMessage message)
    {
        return _state switch
        {
            WorkflowState.Failed => throw new WorkflowException("Workflow cannot be started or resumed while in an Failed state."),
            WorkflowState.Executing => throw new WorkflowException("Workflow cannot be started or resumed while in an Executing state."),
            WorkflowState.Suspended => await ResumeWorkflow(),
            WorkflowState.Created => await InProcessExecution.StreamAsync(workflow, message, checkpointManager),
            WorkflowState.Completed => throw new WorkflowException("Workflow cannot be started or resumed while in an Executing state."),
            _ => throw new WorkflowException("Invalid workflow state.")
        };
    }

    private async Task<Checkpointed<StreamingRun>> ResumeWorkflow()
    {
        if (_checkpointInfo == null)
        {
            throw new WorkflowException("CheckpointInfo is NULL, but checkpoint information is required to resume a workflow.");
        }
        
        return await InProcessExecution.ResumeStreamAsync(workflow, _checkpointInfo,
            checkpointManager);
    }
}