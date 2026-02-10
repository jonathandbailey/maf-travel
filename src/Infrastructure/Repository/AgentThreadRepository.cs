using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repository;

public interface IAgentThreadRepository
{
    Task<string> LoadAsync(string name, string threadId);
    Task SaveAsync(string name, string threadId, string content);
}

public class AgentThreadRepository(ILogger<AgentThreadRepository> logger, IOptions<FileStorageSettings> settings) : IAgentThreadRepository
{
    public async Task<string> LoadAsync(string name, string threadId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(name));
            }

            var filePath = Path.Combine(settings.Value.AgentThreadFolder, threadId, name);

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

    public async Task SaveAsync(string name, string threadId, string content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Content cannot be null or whitespace.", nameof(content));
            }

            var directory = settings.Value.AgentThreadFolder;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fileName = $"{threadId}-{name}.json";
            var filePath = Path.Combine(directory, fileName);

            await File.WriteAllTextAsync(filePath, content);
        }
        catch (ArgumentException exception)
        {
            logger.LogError(exception, "Invalid path or content provided. Path: {path}", name);
            throw;
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogError(exception, "Access denied to file at path: {path}", name);
            throw;
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "IO error while writing file at path: {path}", name);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unknown exception while saving file at path: {path}", name);
            throw;
        }
    }
}