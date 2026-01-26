namespace Infrastructure.Interfaces;

public interface IArtifactRepository
{
    Task SaveAsync(string artifact, Guid id, string path);
    Task SaveAsync<T>(T artifact, string name, string container);
    Task<T> LoadAsync<T>(string name, string container);
    Task<bool> ExistsAsync(string name, string container);
}