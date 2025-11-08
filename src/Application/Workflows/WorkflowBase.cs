using Application.Observability;
using Application.Workflows.Conversations;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace Application.Workflows;

public class WorkflowBase<T> where T : notnull
{
    public CheckpointManager CheckpointManager { get; private set; }

    public CheckpointInfo CheckpointInfo { get; set; }

    public WorkflowState State { get; private set; } = WorkflowState.Initialized;

    protected async Task<Checkpointed<StreamingRun>> CreateStreamingRun(Workflow<T> workflow,
        ChatMessage message)
    {
        using var workflowActivity = Telemetry.StarActivity("Workflow-[create-run]");

        workflowActivity?.SetTag("State:", State);

        switch (State)
        {
            case WorkflowState.Initialized:
                return await StartStreamingRun(workflow, message);
            case WorkflowState.WaitingForUserInput:
                return await ResumeStreamingRun(workflow, message);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task<Checkpointed<StreamingRun>> StartStreamingRun(Workflow<T> workflow,
        ChatMessage message)
    {
        using var workflowActivity = Telemetry.StarActivity("Workflow-[start]");


        return await InProcessExecution.StreamAsync(workflow, message, CheckpointManager);
    }

    private async Task<Checkpointed<StreamingRun>> ResumeStreamingRun(Workflow<T> workflow,
        ChatMessage message)
    {
        var run = await InProcessExecution.ResumeStreamAsync(workflow, CheckpointInfo, CheckpointManager,
            CheckpointInfo.RunId);

        using var workflowActivity = Telemetry.StarActivity("Workflow-[resume]");

        workflowActivity?.SetTag("RunId:", CheckpointInfo.RunId);
        workflowActivity?.SetTag("CheckpointId:", CheckpointInfo.CheckpointId);

        return run;

    }
}