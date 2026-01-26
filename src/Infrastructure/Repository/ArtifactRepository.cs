using Infrastructure.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Repository;

public class ArtifactRepository(IAzureStorageRepository repository, IOptions<AzureStorageSettings> settings) : IArtifactRepository
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<bool> ExistsAsync(string name, string container)
    {
        return await repository.BlobExists(GetFlightSearchFileName(name, container), settings.Value.ContainerName);
    }

    public async Task<T> LoadAsync<T>(string name, string container)
    {
        var filename = GetFlightSearchFileName(name, container);

        var response = await repository.DownloadTextBlobAsync(filename, settings.Value.ContainerName);

        var stateDto = JsonSerializer.Deserialize<T>(response, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Checkpoint Store for session : {name}");


        return stateDto;
    }

    public async Task SaveAsync(string artifact, Guid id, string path)
    {
        await repository.UploadTextBlobAsync(GetFlightSearchFileName(id, path),
            settings.Value.ContainerName,
            artifact, ApplicationJsonContentType);
    }

    public async Task SaveAsync<T>(T artifact, string name, string container)
    {
        var content = JsonSerializer.Serialize(artifact, SerializerOptions);

        await repository.UploadTextBlobAsync(GetFlightSearchFileName(name, container),
            settings.Value.ContainerName,
            content, ApplicationJsonContentType);
    }

    private string GetFlightSearchFileName(string name, string container)
    {
        return $"{container}/{name}.json";
    }

    private string GetFlightSearchFileName(Guid id, string path)
    {
        return $"artifacts/{path}/{id}.json";
    }
}