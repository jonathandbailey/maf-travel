using Microsoft.Agents.AI.Workflows;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Repository;


public class CheckpointRepository(IFileRepository fileRepository) : ICheckpointRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<List<StoreStateDto>> GetAsync(string runId)
    {
        throw new NotImplementedException();
    }

    public async Task SaveAsync(Guid threadId, StoreStateDto storeState)
    {
        var path = $"{threadId}-{storeState.CheckpointInfo.CheckpointId}-{storeState.CheckpointInfo.SessionId}.json";



        var serializedStoreState = JsonSerializer.Serialize(storeState, SerializerOptions);
        
        await fileRepository.SaveAsync(path, serializedStoreState);
    }

    public async Task<StoreStateDto> LoadAsync(Guid threadId, string checkpointId, string runId)
    {
        var path = $"{threadId}-{checkpointId}-{runId}.json";
        var content = await fileRepository.LoadAsync(path);
      
        var storeState = JsonSerializer.Deserialize<StoreStateDto>(content, SerializerOptions);

        if (storeState == null)
        {
            throw new InvalidOperationException($"Failed to deserialize checkpoint data for path: {path}");
        }

        return storeState;
    }
}

public interface ICheckpointRepository
{
    Task<List<StoreStateDto>> GetAsync(string runId);
    Task SaveAsync(Guid threadId, StoreStateDto storeState);
    Task<StoreStateDto> LoadAsync(Guid threadId, string checkpointId, string runId);
}

public class StoreStateDto(CheckpointInfo checkpointInfo, JsonElement jsonElement)
{
    public CheckpointInfo CheckpointInfo { get; init; } = checkpointInfo;

    public JsonElement JsonElement { get; init; } = jsonElement;
}