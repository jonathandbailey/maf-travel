using Infrastructure.Repository.Azure;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Travel.Application.Exceptions;
using Travel.Application.Interfaces;
using Travel.Application.Models;
using Travel.Infrastructure.Common;
using Travel.Infrastructure.Documents;

namespace Travel.Infrastructure.Queries;

public class SessionQuery(
    IAzureStorageQuery storageQuery,
    IOptions<SessionStorageSettings> settings,
    ILogger<SessionQuery> logger) : ISessionQuery
{
    private string ContainerName => settings.Value.ContainerName;

    private static string BlobName(Guid id) => $"{id}.json";

    public async Task<Session> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await storageQuery.BlobExists(BlobName(id), ContainerName))
        {
            logger.LogError("Session {Id} not found in container {Container}", id, ContainerName);
            throw new SessionQueryException($"Session {id} was not found.");
        }

        var json = await storageQuery.DownloadTextBlobAsync(BlobName(id), ContainerName);
        var document = JsonSerializer.Deserialize<SessionDocument>(json, Json.JsonOptions);

        if (document is null)
        {
            logger.LogError("Session {Id} failed to deserialize", id);
            throw new JsonException($"Session {id} failed to deserialize");
        }

        return new Session(document.Id, document.CreatedAt, document.TravelPlanId);
    }

    public async Task<Session?> GetByTravelPlanIdAsync(Guid travelPlanId, CancellationToken cancellationToken = default)
    {
        if (!await storageQuery.ContainerExists(ContainerName))
        {
            return null;
        }

        var blobs = await storageQuery.ListBlobsAsync(ContainerName);

        foreach (var blob in blobs)
        {
            var json = await storageQuery.DownloadTextBlobAsync(blob, ContainerName);

            var document = JsonSerializer.Deserialize<SessionDocument>(json, Json.JsonOptions);

            if (document?.TravelPlanId == travelPlanId)
            {
                return new Session(document.Id, document.CreatedAt, document.TravelPlanId);
            }
        }

        return null;
    }
}
