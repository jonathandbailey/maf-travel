using Infrastructure.Dto;
using Infrastructure.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Travel.Workflows.Models;
using Travel.Workflows.Models.Flights;

namespace Travel.Workflows.Services;


public class FlightService(
    IAzureStorageRepository repository,
    IArtifactRepository artifactRepository,
    IOptions<AzureStorageSeedSettings> settings) : IFlightService
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public async Task<Guid> AddFlightSearchOption(FlightSearchResultDto option)
    {
        var payload = JsonSerializer.Serialize(option, SerializerOptions);

        var id = Guid.NewGuid();

        await artifactRepository.SaveFlightSearchAsync(payload, id);

        return id;
    }
}

public interface IFlightService
{
}