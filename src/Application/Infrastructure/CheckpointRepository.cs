using Application.Workflows.Conversations.Dto;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Infrastructure;

public class CheckpointRepository(IAzureStorageRepository repository, IOptions<AzureStorageSeedSettings> settings) : ICheckpointRepository
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task SaveAsync(StoreStateDto storeState)
    {
        var serializedConversation = JsonSerializer.Serialize(storeState, SerializerOptions);

        await repository.UploadTextBlobAsync(GetCheckpointFileName(
            storeState.CheckpointInfo.CheckpointId, 
            storeState.CheckpointInfo.RunId), 
            settings.Value.ContainerName, 
            serializedConversation, ApplicationJsonContentType);
    }

    public async Task<StoreStateDto> LoadAsync(string checkpointId, string runId)
    {
        var blob = await repository.DownloadTextBlobAsync(GetCheckpointFileName(checkpointId, runId), settings.Value.ContainerName);

        var stateDto = JsonSerializer.Deserialize<StoreStateDto>(blob, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Checkpoint Store for session : {checkpointId}, {runId}");

        
        return stateDto;
    }

    public async Task<List<StoreStateDto>> GetAsync(string runId)
    {
        var blobNames = await repository.ListBlobsAsync(settings.Value.ContainerName, $"{runId}/");
        var checkpoints = new List<StoreStateDto>();

        foreach (var blobName in blobNames)
        {
            var blob = await repository.DownloadTextBlobAsync(blobName, settings.Value.ContainerName);
            var stateDto = JsonSerializer.Deserialize<StoreStateDto>(blob, SerializerOptions);

            if (stateDto == null)
                throw new JsonException($"Failed to deserialize Checkpoint Store from blob: {blobName}");

            checkpoints.Add(stateDto);
        }

        return checkpoints;
    }

    private static string GetCheckpointFileName(string checkpointId, string runId)
    {
        return $"{runId}/{checkpointId}.json";
    }
}

public interface ICheckpointRepository
{
    Task SaveAsync(StoreStateDto storeState);
    Task<StoreStateDto> LoadAsync(string checkpointId, string runId);
    Task<List<StoreStateDto>> GetAsync(string runId);
}