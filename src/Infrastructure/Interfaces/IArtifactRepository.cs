using Infrastructure.Dto;

namespace Infrastructure.Interfaces;

public interface IArtifactRepository
{
    Task SaveAsync(string artifact, string name);
    Task<FlightSearchDto> GetFlightPlanAsync();
    Task<bool> FlightsExistsAsync();
    Task SaveFlightSearchAsync(string artifact, Guid id);
    Task<FlightSearchDto> GetFlightSearch(Guid id);
}