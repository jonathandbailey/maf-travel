using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repository;

public interface IAgentTemplateRepository
{
    Task<string> LoadAsync(string name);
}

public class AgentTemplateRepository(IFileRepository fileRepository, IOptions<FileStorageSettings> settings, ILogger<AgentTemplateRepository> logger) : IAgentTemplateRepository
{
    public Task<string> LoadAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Template name cannot be null or empty.", nameof(name));

        var filePath = Path.IsPathRooted(settings.Value.AgentTemplateFolder)
            ? Path.Combine(settings.Value.AgentTemplateFolder, name)
            : Path.Combine(AppContext.BaseDirectory, settings.Value.AgentTemplateFolder, name);

        logger.LogDebug("Loading agent template {Name} from {Path}", name, filePath);

        return fileRepository.LoadAsync(filePath);
    }
}