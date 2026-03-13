using System.Text.Json;
using Infrastructure.Repository.Azure;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Travel.Application.Interfaces;
using Travel.Application.Models;
using Travel.Infrastructure.Documents;

namespace Travel.Infrastructure.Repositories;

public class SessionRepository(
    IAzureStorageRepository storageRepository,
    IOptions<SessionStorageSettings> settings) : ISessionRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private string ContainerName => settings.Value.ContainerName;

    private static string BlobName(Guid id) => $"{id}.json";

    public async Task AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        await EnsureContainerAsync();
        var document = new SessionDocument(session.Id, session.CreatedAt);
        var json = JsonSerializer.Serialize(document, JsonOptions);
        await storageRepository.UploadTextBlobAsync(BlobName(session.Id), ContainerName, json, "application/json");
    }

    private async Task EnsureContainerAsync()
    {
        if (!await storageRepository.ContainerExists(ContainerName))
            await storageRepository.CreateContainerAsync(ContainerName);
    }
}
