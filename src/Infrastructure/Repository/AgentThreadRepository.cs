using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repository;

public interface IAgentThreadRepository
{
    Task<string> LoadAsync(string name, string threadId);
    Task SaveAsync(string name, string threadId, string content);
}

public class AgentThreadRepository(IFileRepository fileRepository, IOptions<FileStorageSettings> settings, ILogger<AgentThreadRepository> logger) : IAgentThreadRepository
{
    public Task<string> LoadAsync(string name, string threadId)
    {
        ValidateArgs(name, threadId);
        logger.LogDebug("Loading agent thread {Name} for thread {ThreadId}", name, threadId);
        return fileRepository.LoadAsync(GetPath(name, threadId));
    }

    public Task SaveAsync(string name, string threadId, string content)
    {
        ValidateArgs(name, threadId);
        logger.LogDebug("Saving agent thread {Name} for thread {ThreadId}", name, threadId);
        return fileRepository.SaveAsync(GetPath(name, threadId), content);
    }

    private static void ValidateArgs(string name, string threadId)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        if (string.IsNullOrEmpty(threadId))
            throw new ArgumentException("Thread ID cannot be null or empty.", nameof(threadId));
    }

    private string GetPath(string name, string threadId)
    {
        var folder = Path.IsPathRooted(settings.Value.AgentThreadFolder)
            ? settings.Value.AgentThreadFolder
            : Path.Combine(AppContext.BaseDirectory, settings.Value.AgentThreadFolder);

        return Path.Combine(folder, $"{threadId}-{name}.json");
    }
}