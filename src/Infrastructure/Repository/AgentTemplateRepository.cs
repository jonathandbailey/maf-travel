using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repository;

public interface IAgentTemplateRepository
{
    Task<string> LoadAsync(string name);
    Task SaveAsync(string path, string content);
}

public class AgentTemplateRepository(ILogger<AgentTemplateRepository> logger, IOptions<FileStorageSettings> settings) : IAgentTemplateRepository
{
    public async Task<string> LoadAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(name));
            }

            var filePath = Path.Combine(settings.Value.AgentTemplateFolder, name);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found at path: {filePath}", filePath);
            }

            return await File.ReadAllTextAsync(filePath);
        }
        catch (ArgumentException exception)
        {
            logger.LogError(exception, "Invalid path provided: {name}", name);
            throw;
        }
        catch (FileNotFoundException exception)
        {
            logger.LogError(exception, "File not found for template: {name}", name);
            throw;
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogError(exception, "Access denied to file for template: {name}", name);
            throw;
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "IO error while reading file for template: {name}", name);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unknown exception while loading template: {name}", name);
            throw;
        }
    }

    public async Task SaveAsync(string path, string content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Content cannot be null or whitespace.", nameof(content));
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(path, content);
        }
        catch (ArgumentException exception)
        {
            logger.LogError(exception, "Invalid path or content provided. Path: {path}", path);
            throw;
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogError(exception, "Access denied to file at path: {path}", path);
            throw;
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "IO error while writing file at path: {path}", path);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unknown exception while saving file at path: {path}", path);
            throw;
        }
    }
}