using System.Text.Json;
using Infrastructure.Repository.Azure;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Travel.Application.Exceptions;
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
        var document = new SessionDocument(session.Id, session.CreatedAt, session.TravelPlanId);
        var json = JsonSerializer.Serialize(document, JsonOptions);
        await storageRepository.UploadTextBlobAsync(BlobName(session.Id), ContainerName, json, "application/json");
    }

    public async Task<Session> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await storageRepository.BlobExists(BlobName(id), ContainerName))
            throw new NotFoundException($"Session {id} was not found.");

        var json = await storageRepository.DownloadTextBlobAsync(BlobName(id), ContainerName);
        var document = JsonSerializer.Deserialize<SessionDocument>(json, JsonOptions)!;
        return new Session(document.Id, document.CreatedAt, document.TravelPlanId);
    }

    public async Task<Session?> GetByTravelPlanIdAsync(Guid travelPlanId, CancellationToken cancellationToken = default)
    {
        if (!await storageRepository.ContainerExists(ContainerName))
            return null;

        var blobs = await storageRepository.ListBlobsAsync(ContainerName);
        foreach (var blob in blobs)
        {
            var json = await storageRepository.DownloadTextBlobAsync(blob, ContainerName);
            var document = JsonSerializer.Deserialize<SessionDocument>(json, JsonOptions);
            if (document?.TravelPlanId == travelPlanId)
                return new Session(document.Id, document.CreatedAt, document.TravelPlanId);
        }

        return null;
    }

    public async Task UpdateAsync(Session session, CancellationToken cancellationToken = default)
    {
        if (!await storageRepository.BlobExists(BlobName(session.Id), ContainerName))
            throw new NotFoundException($"Session {session.Id} was not found.");

        var document = new SessionDocument(session.Id, session.CreatedAt, session.TravelPlanId);
        var json = JsonSerializer.Serialize(document, JsonOptions);
        await storageRepository.UploadTextBlobAsync(BlobName(session.Id), ContainerName, json, "application/json");
    }

    private async Task EnsureContainerAsync()
    {
        if (!await storageRepository.ContainerExists(ContainerName))
            await storageRepository.CreateContainerAsync(ContainerName);
    }
}
