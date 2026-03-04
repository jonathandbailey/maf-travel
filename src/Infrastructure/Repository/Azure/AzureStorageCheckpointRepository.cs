using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Repository.Azure;

public class AzureStorageCheckpointRepository(
    IAzureStorageRepository storageRepository,
    IOptions<AzureStorageSettings> settings,
    ILogger<AzureStorageCheckpointRepository> logger) : ICheckpointRepository
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
        var containerName = settings.Value.CheckpointContainerName;
        await EnsureContainerExistsAsync(containerName);

        var blobNames = await storageRepository.ListBlobsAsync(containerName, string.Empty);
        blobNames = [..blobNames.Where(b => b.EndsWith($"-{runId}.json"))];

        var results = new List<StoreStateDto>();
        foreach (var blobName in blobNames)
        {
            var content = await storageRepository.DownloadTextBlobAsync(blobName, containerName);
            try
            {
                var storeState = JsonSerializer.Deserialize<StoreStateDto>(content, SerializerOptions);
                if (storeState != null)
                    results.Add(storeState);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize checkpoint blob: {BlobName}", blobName);
                throw;
            }
        }

        return results;
    }

    public async Task SaveAsync(Guid threadId, StoreStateDto storeState)
    {
        var containerName = settings.Value.CheckpointContainerName;
        await EnsureContainerExistsAsync(containerName);

        var blobName = GetBlobName(threadId, storeState.CheckpointInfo.CheckpointId, storeState.CheckpointInfo.SessionId);
        var content = JsonSerializer.Serialize(storeState, SerializerOptions);

        await storageRepository.UploadTextBlobAsync(blobName, containerName, content, "application/json");
    }

    public async Task<StoreStateDto> LoadAsync(Guid threadId, string checkpointId, string runId)
    {
        var containerName = settings.Value.CheckpointContainerName;
        await EnsureContainerExistsAsync(containerName);

        var blobName = GetBlobName(threadId, checkpointId, runId);
        var content = await storageRepository.DownloadTextBlobAsync(blobName, containerName);

        StoreStateDto? storeState;
        try
        {
            storeState = JsonSerializer.Deserialize<StoreStateDto>(content, SerializerOptions);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize checkpoint blob: {BlobName}", blobName);
            throw;
        }

        if (storeState == null)
        {
            logger.LogError("Checkpoint deserialized to null for blob: {BlobName}", blobName);
            throw new InvalidOperationException($"Failed to deserialize checkpoint data for blob: {blobName}");
        }

        return storeState;
    }

    private async Task EnsureContainerExistsAsync(string containerName)
    {
        if (!await storageRepository.ContainerExists(containerName))
            await storageRepository.CreateContainerAsync(containerName);
    }

    private static string GetBlobName(Guid threadId, string checkpointId, string runId) =>
        $"{threadId}/{checkpointId}-{runId}.json";
}
