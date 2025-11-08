using Microsoft.Agents.AI.Workflows;

namespace Application.Workflows.Conversations.Dto;

public class WorkflowStateDto(WorkflowState state, CheckpointInfo checkpointInfo)
{
    public WorkflowState State { get; } = state;

    public CheckpointInfo CheckpointInfo { get; } = checkpointInfo;
}