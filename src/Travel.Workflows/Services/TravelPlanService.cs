using System.ComponentModel;
using Travel.Workflows.Dto;

namespace Travel.Workflows.Services;

public class TravelPlanService : ITravelPlanService
{
    public Task Update(TravelPlanDto updateDto)
    {
        throw new NotImplementedException();
    }
}

public interface ITravelPlanService
{
    [Description("Updates a Travel Plan")]
    Task Update(
       [Description("The Travel Plan Dto containing the update.")] TravelPlanDto updateDto);
}