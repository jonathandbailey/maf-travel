using Application.Dto;
using Microsoft.Agents.AI.Workflows;

namespace Application.Interfaces;

public interface IWorkflowRepository
{
    Task SaveAsync(Guid userId, Guid sessionId, WorkflowState state, CheckpointInfo? checkpointInfo);
   
    Task<WorkflowStateDto> LoadAsync(Guid userId, Guid sessionId);
}