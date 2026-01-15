namespace Workflows.Repository;

public interface ICheckpointRepository
{
    Task SaveAsync(Guid userId, Guid sessionId, StoreStateDto storeState);
    Task<StoreStateDto> LoadAsync(Guid userId, Guid sessionId, string checkpointId, string runId);
    Task<List<StoreStateDto>> GetAsync(Guid userId, Guid sessionId, string runId);
    Task SaveAsync(string threadId, StoreStateDto storeState);
    Task<StoreStateDto> LoadAsync(string threadId, string checkpointId, string runId);
    Task<List<StoreStateDto>> GetAsync(string runId);
}