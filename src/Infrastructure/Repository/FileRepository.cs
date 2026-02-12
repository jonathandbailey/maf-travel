namespace Infrastructure.Repository;

using Microsoft.Extensions.Logging;

public class FileRepository(ILogger<FileRepository> logger) : IFileRepository
{
    public async Task<string> LoadAsync(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File not found at path: {path}", path);
            }

            return await File.ReadAllTextAsync(path);
        }
        catch (ArgumentException exception)
        {
            logger.LogError(exception, "Invalid path provided: {Path}", path);
            throw;
        }
        catch (FileNotFoundException exception)
        {
            logger.LogError(exception, "File not found at path: {Path}", path);
            throw;
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogError(exception, "Access denied to file at path: {Path}", path);
            throw;
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "IO error while reading file at path: {Path}", path);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unknown exception while loading file at path: {Path}", path);
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
            logger.LogError(exception, "Invalid path or content provided. Path: {Path}", path);
            throw;
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogError(exception, "Access denied to file at path: {Path}", path);
            throw;
        }
        catch (IOException exception)
        {
            logger.LogError(exception, "IO error while writing file at path: {Path}", path);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unknown exception while saving file at path: {Path}", path);
            throw;
        }
    }
}

public interface IFileRepository
{
    public Task<string> LoadAsync(string path);

    public Task SaveAsync(string path, string content);
}