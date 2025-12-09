using Application.Models;

namespace Application.Workflows.Dto;

public class CreatePlanRequestDto(TravelPlan travelPlan)
{
    public TravelPlan TravelPlan => travelPlan;
}