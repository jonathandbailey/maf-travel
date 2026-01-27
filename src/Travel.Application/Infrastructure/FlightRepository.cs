using Infrastructure.Interfaces;
using Travel.Application.Api.Infrastructure.Documents;
using Travel.Application.Api.Infrastructure.Mappers;
using Travel.Application.Domain.Flights;

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
        var document = flightSearch.ToDocument();

        await artifactRepository.SaveAsync(document, document.Id.ToString(), GetResourceName());

        return document.Id;
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