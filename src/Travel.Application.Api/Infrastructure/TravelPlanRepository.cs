using Infrastructure.Interfaces;
using Travel.Application.Api.Domain;
using Travel.Application.Api.Infrastructure.Documents;
using Travel.Application.Api.Infrastructure.Mappers;

namespace Travel.Application.Api.Infrastructure;

public class TravelPlanPlanRepository(IArtifactRepository artifactRepository) : ITravelPlanRepository
{
    private const string StoragePathTemplate = "{0}/travel-plans";

    public async Task<TravelPlan> LoadAsync(Guid userId, Guid travelPlanId)
    {
        var document = await artifactRepository.LoadAsync<TravelPlanDocument>(travelPlanId.ToString(), GetStorageFileName(userId));
       
        return document.ToDomain();
    }

    public async Task SaveAsync(TravelPlan travelPlan, Guid userId)
    {
        await artifactRepository.SaveAsync(travelPlan, travelPlan.Id.ToString(), GetStorageFileName(userId));
    }

    private static string GetStorageFileName(Guid userId)
    {
        return string.Format(StoragePathTemplate, userId);
    }
}

public interface ITravelPlanRepository
{
    Task SaveAsync(TravelPlan state, Guid userId);
    Task<TravelPlan> LoadAsync(Guid userId, Guid travelPlanId);
}