using Travel.Domain.Aggregates;

namespace Travel.Application.Interfaces;

public interface ITravelPlanRepository
{
    Task<TravelPlan> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TravelPlan>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TravelPlan plan, CancellationToken cancellationToken = default);
    Task UpdateAsync(TravelPlan plan, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
