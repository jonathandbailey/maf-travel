using Infrastructure.Repository.Azure;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Travel.Application.Exceptions;
using Travel.Application.Interfaces;
using Travel.Domain.Aggregates;
using Travel.Infrastructure.Common;
using Travel.Infrastructure.Documents;

namespace Travel.Infrastructure.Queries;


public class TravelPlanQuery(IAzureStorageRepository storageRepository,
    IOptions<TravelPlanStorageSettings> settings,
    ILogger<TravelPlanQuery> logger) : ITravelPlanQuery
{
    private string ContainerName => settings.Value.ContainerName;

    private static string BlobName(Guid id) => $"{id}.json";

    public async Task<TravelPlan> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var blobName = BlobName(id);

        if (!await storageRepository.BlobExists(blobName, ContainerName))
        {
            logger.LogError("TravelPlan {id} not found in container {ContainerName}", id, ContainerName);
            throw new NotFoundException($"TravelPlan {id} not found in container {ContainerName}");
        }

        var json = await storageRepository.DownloadTextBlobAsync(blobName, ContainerName);

        var document = JsonSerializer.Deserialize<TravelPlanDocument>(json, Json.JsonOptions);

        if (document is null)
        {
            logger.LogError("Travel Plan {id} failed to deserialize", id);
            throw new JsonException($"Travel Plan {id} failed to deserialize");
        }

        return ToDomain(document);
    }

    public async Task<IReadOnlyList<TravelPlan>> ListAsync(CancellationToken cancellationToken = default)
    {
        var blobs = await storageRepository.ListBlobsAsync(ContainerName);

        var plans = new List<TravelPlan>(blobs.Count);

        foreach (var blob in blobs)
        {
            var json = await storageRepository.DownloadTextBlobAsync(blob, ContainerName);

            var document = JsonSerializer.Deserialize<TravelPlanDocument>(json, Json.JsonOptions);
            
            if (document is null)
            {
                logger.LogError("Failed to deserialize TravelPlan blob {BlobName} in {Container}", blob, ContainerName);
                continue;
            }

            plans.Add(ToDomain(document));
        }

        return plans;
    }

    private static TravelPlan ToDomain(TravelPlanDocument doc) =>
        TravelPlan.Reconstitute(doc.Id, doc.Origin, doc.Destination, doc.NumberOfTravelers, doc.StartDate, doc.EndDate);
}