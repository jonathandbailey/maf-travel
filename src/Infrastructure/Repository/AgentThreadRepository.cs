using Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repository;

public interface IAgentThreadRepository
{
    Task<string> LoadAsync(string name, string threadId);
    Task SaveAsync(string name, string threadId, string content);
}

public class AgentThreadRepository(IFileRepository fileRepository, IOptions<FileStorageSettings> settings) : IAgentThreadRepository
{
    public Task<string> LoadAsync(string name, string threadId)
    {
        var folder = Path.IsPathRooted(settings.Value.AgentThreadFolder)
            ? settings.Value.AgentThreadFolder
            : Path.Combine(AppContext.BaseDirectory, settings.Value.AgentThreadFolder);

        return fileRepository.LoadAsync(Path.Combine(folder, threadId, name));
    }

    public Task SaveAsync(string name, string threadId, string content)
    {
        var folder = Path.IsPathRooted(settings.Value.AgentThreadFolder)
            ? settings.Value.AgentThreadFolder
            : Path.Combine(AppContext.BaseDirectory, settings.Value.AgentThreadFolder);

        return fileRepository.SaveAsync(Path.Combine(folder, $"{threadId}-{name}.json"), content);
    }
}