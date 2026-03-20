using Infrastructure.Repository.Azure;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Travel.Application.Exceptions;
using Travel.Application.Interfaces;
using Travel.Domain.Aggregates.FlightSearch;
using Travel.Infrastructure.Common;
using Travel.Infrastructure.Documents;

namespace Travel.Infrastructure.Queries;

public class FlightSearchQuery(
    IAzureStorageRepository storageRepository,
    IOptions<FlightSearchStorageSettings> settings,
    ILogger<FlightSearchQuery> logger) : IFlightSearchQuery
{
    private string ContainerName => settings.Value.ContainerName;

    private static string BlobName(Guid id) => $"{id}.json";

    public async Task<FlightSearch> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var blobName = BlobName(id);

        if (!await storageRepository.BlobExists(blobName, ContainerName))
        {
            logger.LogError("FlightSearch {Id} not found in container {Container}", id, ContainerName);
            throw new NotFoundException($"FlightSearch {id} not found.");
        }

        var json = await storageRepository.DownloadTextBlobAsync(blobName, ContainerName);
        var document = JsonSerializer.Deserialize<FlightSearchDocument>(json, Json.JsonOptions);

        if (document is null)
        {
            logger.LogError("FlightSearch {Id} failed to deserialize", id);
            throw new JsonException($"FlightSearch {id} failed to deserialize");
        }

        return ToDomain(document);
    }

    public async Task<IReadOnlyList<FlightSearch>> ListAsync(CancellationToken cancellationToken = default)
    {
        var blobs = await storageRepository.ListBlobsAsync(ContainerName);
        var searches = new List<FlightSearch>(blobs.Count);

        foreach (var blob in blobs)
        {
            var json = await storageRepository.DownloadTextBlobAsync(blob, ContainerName);
            var document = JsonSerializer.Deserialize<FlightSearchDocument>(json, Json.JsonOptions);

            if (document is null)
            {
                logger.LogWarning("Failed to deserialize FlightSearch blob {BlobName} in {Container}", blob, ContainerName);
            }
            else
            {
                searches.Add(ToDomain(document));
            }
        }

        return searches;
    }

    private static FlightSearch ToDomain(FlightSearchDocument doc) =>
        FlightSearch.Reconstitute(
            doc.Id,
            doc.CreatedAt,
            doc.FlightOptions
                .Select(o => FlightOption.Reconstitute(
                    o.Id,
                    o.FlightNumber,
                    o.Airline,
                    o.Origin,
                    o.Destination,
                    o.DepartureTime,
                    o.ArrivalTime,
                    o.PricePerPerson,
                    o.AvailableSeats))
                .ToList());
}
