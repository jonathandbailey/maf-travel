using System.Runtime.CompilerServices;
using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Common;
using Travel.Workflows.Common.Exceptions;
using Travel.Workflows.TravelPlanCriteria.Dto;
using Travel.Workflows.TravelPlanCriteria.Events;

namespace Travel.Workflows.TravelPlanCriteria.Services;

public class TravelPlanningRunner(Workflow workflow, CheckpointManager checkpointManager, WorkflowSession session)
{
    private WorkflowSession _session = session;

    public WorkflowSession Session => _session;

    public async IAsyncEnumerable<WorkflowEvent> WatchStreamAsync(WorkflowRunRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Message.Text))
        {
            throw new WorkflowException("The request message is empty. This workflow cannot process an empty message");
        }
        
        var run = await CreateWorkflowRun(request);

        await foreach (var evt in run.WatchStreamAsync(cancellationToken))
        {
            switch (evt)
            {
                case SuperStepCompletedEvent superStepCompletedEvt:
                    HandleSuperStepCompletedEvent(superStepCompletedEvt);
                    break;
                
                case RequestInfoEvent requestInfoEvent when Session.State == WorkflowState.Suspended:
                    var resp = requestInfoEvent.Request.CreateResponse(new InformationResponse(request.Message));

                    TransitionTo(WorkflowState.Executing);
                    await run.SendResponseAsync(resp);
                    break;

                case RequestInfoEvent when Session.State == WorkflowState.Executing:
                    TransitionTo(WorkflowState.Suspended);
                    yield return evt;
                    yield break;

                case TravelPlanStatusUpdateEvent:
                case TravelPlanUpdateEvent:
                    yield return evt;
                    break;

                case TravelPlanningCompleteEvent:
                    TransitionTo(WorkflowState.Completed);
                    yield return evt;
                    break;
                case WorkflowErrorEvent:
                case ExecutorFailedEvent:
                    TransitionTo(WorkflowState.Failed);
                    yield return evt;
                    break;
            }
        }
    }

    private void TransitionTo(WorkflowState state) => _session = _session with { State = state };

    private void UpdateCheckpoint(CheckpointInfo checkpoint) => _session = _session with { LastCheckpoint = checkpoint };

    private void HandleSuperStepCompletedEvent(SuperStepCompletedEvent superStepCompletedEvt)
    {
        var checkpoint = superStepCompletedEvt.CompletionInfo?.Checkpoint;

        if (checkpoint != null)
        {
            UpdateCheckpoint(checkpoint);
        }
    }

    private async ValueTask<StreamingRun> CreateWorkflowRun(WorkflowRunRequest request)
    {
        switch (_session.State)
        {
            case WorkflowState.Created:
                TransitionTo(WorkflowState.Executing);
                return await InProcessExecution.RunStreamingAsync(workflow, request, checkpointManager);

            case WorkflowState.Suspended when _session.LastCheckpoint != null:
                return await InProcessExecution.ResumeStreamingAsync(workflow, _session.LastCheckpoint, checkpointManager);

            case WorkflowState.Suspended:
                throw new WorkflowException("Cannot resume: workflow is suspended but has no checkpoint.");

            case WorkflowState.Executing:
                throw new WorkflowException("Workflow is already executing.");

            case WorkflowState.Completed:
                TransitionTo(WorkflowState.Executing);
                return await InProcessExecution.RunStreamingAsync(workflow, request, checkpointManager);

            case WorkflowState.Failed:
                throw new WorkflowException("Workflow has failed and cannot be restarted.");

            default:
                throw new WorkflowException("Invalid workflow state.");
        }
    }
}
