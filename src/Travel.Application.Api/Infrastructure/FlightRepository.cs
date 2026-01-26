using System.Text.Json;
using Infrastructure.Interfaces;
using Travel.Application.Api.Domain.Flights;
using Travel.Application.Api.Infrastructure.Documents;
using Travel.Application.Api.Infrastructure.Mappers;

namespace Travel.Application.Api.Infrastructure;

public class FlightRepository(IArtifactRepository artifactRepository) : IFlightRepository
{
    public async Task<FlightSearch> GetFlightSearch(Guid userId, Guid searchId)
    {
        var json = await artifactRepository.LoadAsync(searchId, "flights");

        var document = JsonSerializer.Deserialize<FlightSearchDocument>(json);

        if (document == null)
        {
            throw new ArgumentException("Failed to deserialize flight search");
        }

        return document.ToDomain();
    }

    public async Task<Guid> SaveFlightSearch(FlightSearch flightSearch)
    {
        var id = Guid.NewGuid();
        var document = flightSearch.ToDocument();
        var payload = JsonSerializer.Serialize(document);
        await artifactRepository.SaveAsync(payload, id, "flights");

        return id;
    }
}

public interface IFlightRepository
{
    Task<FlightSearch> GetFlightSearch(Guid userId, Guid searchId);
    Task<Guid> SaveFlightSearch(FlightSearch flightSearch);
}