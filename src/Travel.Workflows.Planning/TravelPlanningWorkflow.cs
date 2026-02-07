using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Workflows.Planning.Dto;

namespace Travel.Workflows.Planning;

public class TravelPlanningWorkflow(Workflow workflow,
    CheckpointManager checkpointManager, CheckpointInfo? checkpointInfo = null, WorkflowState state = WorkflowState.Created)
{
    private CheckpointInfo? _checkpointInfo = checkpointInfo;
    private WorkflowState _state = state;

    public CheckpointInfo? CheckpointInfo => _checkpointInfo;

    public WorkflowState State => _state;


    public async IAsyncEnumerable<WorkflowEvent> Run(ChatMessage message)
    {
        var run = await CreateWorkflowRun(message);

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
                        var resp = requestInfoEvent.Request.CreateResponse(new InformationResponse(message));

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
            checkpointManager, _checkpointInfo.RunId);
    }
}