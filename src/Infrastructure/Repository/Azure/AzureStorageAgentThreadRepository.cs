using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repository.Azure;

public class AzureStorageAgentThreadRepository(
    IAzureStorageRepository storageRepository,
    IOptions<AzureStorageSettings> settings,
    ILogger<AzureStorageAgentThreadRepository> logger) : IAgentThreadRepository
{
    public async Task<string> LoadAsync(string name, string threadId)
    {
        ValidateArgs(name, threadId);
        logger.LogDebug("Loading agent thread {Name} for thread {ThreadId}", name, threadId);
        var containerName = settings.Value.AgentThreadContainerName;
        await EnsureContainerExistsAsync(containerName);
        return await storageRepository.DownloadTextBlobAsync(GetBlobName(name, threadId), containerName);
    }

    public async Task SaveAsync(string name, string threadId, string content)
    {
        ValidateArgs(name, threadId);
        logger.LogDebug("Saving agent thread {Name} for thread {ThreadId}", name, threadId);
        var containerName = settings.Value.AgentThreadContainerName;
        await EnsureContainerExistsAsync(containerName);
        await storageRepository.UploadTextBlobAsync(GetBlobName(name, threadId), containerName, content, "application/json");
    }

    private async Task EnsureContainerExistsAsync(string containerName)
    {
        if (!await storageRepository.ContainerExists(containerName))
            await storageRepository.CreateContainerAsync(containerName);
    }

    private static void ValidateArgs(string name, string threadId)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        if (string.IsNullOrEmpty(threadId))
            throw new ArgumentException("Thread ID cannot be null or empty.", nameof(threadId));
    }

    private static string GetBlobName(string name, string threadId) => $"{threadId}/{name}.json";
}
