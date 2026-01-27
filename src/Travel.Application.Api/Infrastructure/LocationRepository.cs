using Travel.Application.Api.Domain;
using Travel.Application.Api.Infrastructure.Seed;

namespace Travel.Application.Api.Infrastructure;

public class LocationRepository : ILocationRepository
{
    public async Task<List<Location>> GetLocationsByCityAsync(string city)
    {
        return await Task.FromResult(DataRegistry.EuropeanCities
            .Where(location => location.City.Equals(city, StringComparison.OrdinalIgnoreCase))
            .ToList());
    }
}

public interface ILocationRepository
{
    Task<List<Location>> GetLocationsByCityAsync(string city);
}