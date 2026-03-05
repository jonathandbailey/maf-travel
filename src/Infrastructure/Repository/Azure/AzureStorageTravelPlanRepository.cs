using System.Text.Json;
using Infrastructure.Repository.Entities;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repository.Azure;

public class AzureStorageTravelPlanRepository(
    IAzureStorageRepository storageRepository,
    IOptions<TravelPlanStorageSettings> settings,
    ILogger<AzureStorageTravelPlanRepository> logger)
    : ITravelPlanRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private string ContainerName => settings.Value.ContainerName;

    private static string BlobName(Guid planId) => $"{planId}.json";

    public async Task<TravelPlanEntity?> GetAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        Verify.NotEmpty(planId);

        var blobName = BlobName(planId);

        var exists = await storageRepository.BlobExists(blobName, ContainerName);
        if (!exists)
        {
            logger.LogDebug("Travel plan {PlanId} not found in container {Container}", planId, ContainerName);
            return null;
        }

        var json = await storageRepository.DownloadTextBlobAsync(blobName, ContainerName);
        return JsonSerializer.Deserialize<TravelPlanEntity>(json, JsonOptions);
    }

    public async Task SaveAsync(TravelPlanEntity entity, CancellationToken cancellationToken = default)
    {
        Verify.NotNull(entity);
        Verify.NotEmpty(entity.Id);

        var blobName = BlobName(entity.Id);
        var json = JsonSerializer.Serialize(entity, JsonOptions);

        await storageRepository.UploadTextBlobAsync(blobName, ContainerName, json, "application/json");
        logger.LogDebug("Travel plan {PlanId} saved to container {Container}", entity.Id, ContainerName);
    }

    public async Task DeleteAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        Verify.NotEmpty(planId);

        await storageRepository.DeleteBlobAsync(BlobName(planId), ContainerName);
        logger.LogDebug("Travel plan {PlanId} deleted from container {Container}", planId, ContainerName);
    }

    public async Task<IReadOnlyList<Guid>> ListIdsAsync(CancellationToken cancellationToken = default)
    {
        var blobNames = await storageRepository.ListBlobsAsync(ContainerName);

        var ids = new List<Guid>();
        foreach (var name in blobNames)
        {
            var stem = Path.GetFileNameWithoutExtension(name);
            if (Guid.TryParse(stem, out var id))
                ids.Add(id);
        }

        return ids;
    }
}
