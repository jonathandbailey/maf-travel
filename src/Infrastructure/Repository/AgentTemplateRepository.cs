using Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repository;

public interface IAgentTemplateRepository
{
    Task<string> LoadAsync(string name);
    
}

public class AgentTemplateRepository(IFileRepository fileRepository, IOptions<FileStorageSettings> settings) : IAgentTemplateRepository
{
    public Task<string> LoadAsync(string name)
    {
        var filePath = Path.IsPathRooted(settings.Value.AgentTemplateFolder)
            ? Path.Combine(settings.Value.AgentTemplateFolder, name)
            : Path.Combine(AppContext.BaseDirectory, settings.Value.AgentTemplateFolder, name);

        return fileRepository.LoadAsync(filePath);
    }

    
}