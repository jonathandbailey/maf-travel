using Infrastructure.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Repository;

public class ArtifactRepository2(IFileRepository fileRepository, IOptions<AzureStorageSettings> settings) : IArtifactRepository
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<bool> ExistsAsync(string name, string container)
    {
        var filePath = GetArtifactFilePath(name, container);
        return Task.FromResult(File.Exists(filePath));
    }

    public async Task<T> LoadAsync<T>(string name, string container)
    {
        var filePath = GetArtifactFilePath(name, container);

        var response = await fileRepository.LoadAsync(filePath);

        var stateDto = JsonSerializer.Deserialize<T>(response, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Checkpoint Store for session : {name}");

        return stateDto;
    }

    public async Task SaveAsync(string artifact, Guid id, string path)
    {
        var filePath = GetArtifactFilePathById(id, path);
        await fileRepository.SaveAsync(filePath, artifact);
    }

    public async Task SaveAsync<T>(T artifact, string name, string container)
    {
        var content = JsonSerializer.Serialize(artifact, SerializerOptions);
        var filePath = GetArtifactFilePath(name, container);
        await fileRepository.SaveAsync(filePath, content);
    }

    private string GetArtifactFilePath(string name, string container)
    {
        var baseFolder = settings.Value.ContainerName;
        return Path.Combine(baseFolder, container, $"{name}.json");
    }

    private string GetArtifactFilePathById(Guid id, string path)
    {
        var baseFolder = settings.Value.ContainerName;
        return Path.Combine(baseFolder, "artifacts", path, $"{id}.json");
    }
}