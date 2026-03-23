using Travel.Application.Models;

namespace Travel.Application.Interfaces;

public interface ITravelPlanQuery
{
    Task<TravelPlanReadModel> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TravelPlanReadModel>> ListAsync(CancellationToken cancellationToken = default);
}