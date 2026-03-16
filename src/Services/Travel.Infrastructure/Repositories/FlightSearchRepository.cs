using System.Text.Json;
using Infrastructure.Repository.Azure;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Travel.Application.Exceptions;
using Travel.Application.Interfaces;
using Travel.Domain.Aggregates.FlightSearch;
using Travel.Infrastructure.Documents;

namespace Travel.Infrastructure.Repositories;

public class FlightSearchRepository(
    IAzureStorageRepository storageRepository,
    IOptions<FlightSearchStorageSettings> settings,
    ILogger<FlightSearchRepository> logger) : IFlightSearchRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private string ContainerName => settings.Value.ContainerName;

    private static string BlobName(Guid id) => $"{id}.json";

    public async Task<FlightSearch> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var blobName = BlobName(id);
        if (!await storageRepository.BlobExists(blobName, ContainerName))
            throw new NotFoundException($"FlightSearch {id} not found.");

        var json = await storageRepository.DownloadTextBlobAsync(blobName, ContainerName);
        var document = JsonSerializer.Deserialize<FlightSearchDocument>(json, JsonOptions);
        return document is null
            ? throw new NotFoundException($"FlightSearch {id} not found.")
            : ToDomain(document);
    }

    public async Task<IReadOnlyList<FlightSearch>> ListAsync(CancellationToken cancellationToken = default)
    {
        var blobs = await storageRepository.ListBlobsAsync(ContainerName);
        var searches = new List<FlightSearch>(blobs.Count);

        foreach (var blob in blobs)
        {
            var json = await storageRepository.DownloadTextBlobAsync(blob, ContainerName);
            var document = JsonSerializer.Deserialize<FlightSearchDocument>(json, JsonOptions);
            if (document is null)
                logger.LogWarning("Failed to deserialize FlightSearch blob {BlobName} in {Container}", blob, ContainerName);
            else
                searches.Add(ToDomain(document));
        }

        return searches;
    }

    public async Task AddAsync(FlightSearch search, CancellationToken cancellationToken = default)
    {
        await EnsureContainerAsync();
        var json = JsonSerializer.Serialize(ToDocument(search), JsonOptions);
        await storageRepository.UploadTextBlobAsync(BlobName(search.Id), ContainerName, json, "application/json");
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await storageRepository.BlobExists(BlobName(id), ContainerName))
            throw new NotFoundException($"FlightSearch {id} not found.");

        await storageRepository.DeleteBlobAsync(BlobName(id), ContainerName);
    }

    private async Task EnsureContainerAsync()
    {
        if (!await storageRepository.ContainerExists(ContainerName))
            await storageRepository.CreateContainerAsync(ContainerName);
    }

    private static FlightSearchDocument ToDocument(FlightSearch search) =>
        new(
            search.Id,
            search.CreatedAt,
            search.FlightOptions
                .Select(o => new FlightOptionDocument(
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
