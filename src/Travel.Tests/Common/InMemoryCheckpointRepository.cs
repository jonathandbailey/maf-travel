using Infrastructure.Repository;

namespace Travel.Tests.Common;

public class InMemoryCheckpointRepository : ICheckpointRepository
{
    private readonly Dictionary<string, StoreStateDto> _store = new();
    
    public async Task<List<StoreStateDto>> GetAsync(string runId)
    {
        throw new NotImplementedException();
    }

    public async Task SaveAsync(Guid threadId, StoreStateDto storeState)
    {
        var path = $"{threadId}-{storeState.CheckpointInfo.CheckpointId}-{storeState.CheckpointInfo.RunId}.json";

        _store[path] = storeState;

    }

    public async Task<StoreStateDto> LoadAsync(Guid threadId, string checkpointId, string runId)
    {
        var path = $"{threadId}-{checkpointId}-{runId}.json";

        return _store[path];
    }
}