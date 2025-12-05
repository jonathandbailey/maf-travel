using Application.Models;

namespace Application.Workflows.ReWoo.Dto;

public class CreatePlanRequestDto(TravelPlan travelPlan)
{
    public TravelPlan TravelPlan => travelPlan;
}