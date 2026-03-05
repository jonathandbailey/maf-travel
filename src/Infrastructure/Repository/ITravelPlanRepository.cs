using Infrastructure.Repository.Entities;

namespace Infrastructure.Repository;

public interface ITravelPlanRepository
{
    Task<TravelPlanEntity?> GetAsync(Guid planId, CancellationToken cancellationToken = default);
    Task SaveAsync(TravelPlanEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid planId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> ListIdsAsync(CancellationToken cancellationToken = default);
}
