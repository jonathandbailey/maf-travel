using Travel.Domain.Aggregates;

namespace Travel.Application.Interfaces;

public interface ITravelPlanQuery
{
    Task<TravelPlan> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TravelPlan>> ListAsync(CancellationToken cancellationToken = default);
}