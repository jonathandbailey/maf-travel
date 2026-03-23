using Travel.Application.Exceptions;
using Travel.Application.Interfaces;
using Travel.Domain.Aggregates;

namespace Travel.Tests.Shared;

public class InMemoryTravelPlanRepository : ITravelPlanRepository
{
    private readonly Dictionary<Guid, TravelPlan> _store = [];

    public Task<TravelPlan> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetValue(id, out var plan))
            throw new TravelPlanUpdateException($"TravelPlan {id} not found.");

        return Task.FromResult(plan);
    }

    public Task<IReadOnlyList<TravelPlan>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<TravelPlan>>(_store.Values.ToList());

    public Task AddAsync(TravelPlan plan, CancellationToken cancellationToken = default)
    {
        _store[plan.Id] = plan;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TravelPlan plan, CancellationToken cancellationToken = default)
    {
        if (!_store.ContainsKey(plan.Id))
            throw new TravelPlanUpdateException($"TravelPlan {plan.Id} not found.");

        _store[plan.Id] = plan;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_store.Remove(id))
            throw new TravelPlanUpdateException($"TravelPlan {id} not found.");

        return Task.CompletedTask;
    }
}
