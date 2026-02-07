using System.ComponentModel;
using Travel.Workflows.Planning.Dto;

namespace Travel.Workflows.Planning.Services;

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