using Travel.Application.Models;

namespace Travel.Application.Interfaces;

public interface ISessionQuery
{
    Task<Session> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Session?> GetByTravelPlanIdAsync(Guid travelPlanId, CancellationToken cancellationToken = default);
}
