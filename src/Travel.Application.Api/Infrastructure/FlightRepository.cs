using Infrastructure.Interfaces;
using Travel.Application.Api.Domain.Flights;
using Travel.Application.Api.Infrastructure.Documents;
using Travel.Application.Api.Infrastructure.Mappers;

namespace Travel.Application.Api.Infrastructure;

public class FlightRepository(IArtifactRepository artifactRepository) : IFlightRepository
{
    public async Task<FlightSearch> GetFlightSearch(Guid userId, Guid searchId)
    {
        var document = await artifactRepository.LoadAsync<FlightSearchDocument>(searchId.ToString(), GetResourceName());

        return document.ToDomain();
    }

    public async Task<Guid> SaveFlightSearch(FlightSearch flightSearch)
    {
        var id = Guid.NewGuid();
        var document = flightSearch.ToDocument();
        
        await artifactRepository.SaveAsync(document, id.ToString(), GetResourceName());

        return id;
    }

    private static string GetResourceName()
    {
        return $"artifacts/flights";
    }
}

public interface IFlightRepository
{
    Task<FlightSearch> GetFlightSearch(Guid userId, Guid searchId);
    Task<Guid> SaveFlightSearch(FlightSearch flightSearch);
}