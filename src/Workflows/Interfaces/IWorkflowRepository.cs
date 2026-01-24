using Microsoft.Agents.AI.Workflows;
using Workflows.Dto;

namespace Workflows.Interfaces;

public interface IWorkflowRepository
{
    Task<WorkflowStateDto> LoadAsync(Guid threadId);
    Task SaveAsync(Guid threadId, WorkflowState state, CheckpointInfo? checkpointInfo);
}