using System.Text.Json;
using Infrastructure.Repository.Azure;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Travel.Application.Exceptions;
using Travel.Application.Interfaces;
using Travel.Domain.Aggregates;
using Travel.Infrastructure.Common;
using Travel.Infrastructure.Documents;

namespace Travel.Infrastructure.Repositories;

public class TravelPlanRepository(
    IAzureStorageRepository storageRepository,
    IOptions<TravelPlanStorageSettings> settings,
    ILogger<TravelPlanRepository> logger) : ITravelPlanRepository
{
    private const string ApplicationJson = Json.ApplicationJson;
    private string ContainerName => settings.Value.ContainerName;

    private static string BlobName(Guid id) => $"{id}.json";

    public async Task<TravelPlan> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var blobName = BlobName(id);

        if (!await storageRepository.BlobExists(blobName, ContainerName))
        {
            logger.LogError("TravelPlan {id} not found in container {ContainerName}", id, ContainerName);
            throw new TravelPlanUpdateException($"TravelPlan {id} not found in container {ContainerName}");
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

    public async Task AddAsync(TravelPlan plan, CancellationToken cancellationToken = default)
    {
        await EnsureContainerAsync();

        var json = JsonSerializer.Serialize(ToDocument(plan), Json.JsonOptions);
        
        await storageRepository.UploadTextBlobAsync(BlobName(plan.Id), ContainerName, json, ApplicationJson);
    }

    public async Task UpdateAsync(TravelPlan plan, CancellationToken cancellationToken = default)
    {
        if (!await storageRepository.BlobExists(BlobName(plan.Id), ContainerName))
        {
            logger.LogError("TravelPlan {Id} not found in container {Container}", plan.Id, ContainerName);
            throw new TravelPlanUpdateException($"TravelPlan {plan.Id} not found.");
        }

        var json = JsonSerializer.Serialize(ToDocument(plan), Json.JsonOptions);

        await storageRepository.UploadTextBlobAsync(BlobName(plan.Id), ContainerName, json, ApplicationJson);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await storageRepository.BlobExists(BlobName(id), ContainerName))
        {
            logger.LogError("TravelPlan {Id} not found in container {Container}", id, ContainerName);
            throw new TravelPlanUpdateException($"TravelPlan {id} not found.");
        }

        await storageRepository.DeleteBlobAsync(BlobName(id), ContainerName);
    }

    private async Task EnsureContainerAsync()
    {
        if (!await storageRepository.ContainerExists(ContainerName))
        {
            await storageRepository.CreateContainerAsync(ContainerName);
        }
    }

    private static TravelPlanDocument ToDocument(TravelPlan plan) =>
        new(plan.Id, plan.Origin, plan.Destination, plan.NumberOfTravelers, plan.StartDate, plan.EndDate);

    private static TravelPlan ToDomain(TravelPlanDocument doc) =>
        TravelPlan.Reconstitute(doc.Id, doc.Origin, doc.Destination, doc.NumberOfTravelers, doc.StartDate, doc.EndDate);
}
