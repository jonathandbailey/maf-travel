using Microsoft.Agents.AI.Workflows;
using Workflows.Dto;

namespace Workflows.Repository;

public interface IWorkflowRepository
{
    Task SaveAsync(Guid userId, Guid sessionId, WorkflowState state, CheckpointInfo? checkpointInfo);
   
    Task<WorkflowStateDto> LoadAsync(Guid userId, Guid sessionId);
    Task<WorkflowStateDto> LoadAsync(string threadId);
    Task SaveAsync(string threadId, WorkflowState state, CheckpointInfo? checkpointInfo);
}