using Microsoft.Agents.AI.Workflows;

namespace Application.Workflows.Conversations.Dto;

public class WorkflowStateDto(WorkflowState state, CheckpointInfo checkpointInfo, List<StoreStateDto> storeStates)
{
    public WorkflowState State { get; } = state;

    public CheckpointInfo CheckpointInfo { get; } = checkpointInfo;

    public List<StoreStateDto> StoreStates { get; } = storeStates;
}