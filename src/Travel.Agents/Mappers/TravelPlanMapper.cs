using Infrastructure.Repository.Entities;
using Travel.Agents.Dto;

namespace Travel.Agents.Mappers;

public static class TravelPlanMapper
{
    public static TravelPlanEntity ToEntity(Guid planId, TravelPlanState state) => new()
    {
        Id = planId,
        Origin = state.Origin,
        Destination = state.Destination,
        NumberOfTravelers = state.NumberOfTravelers,
        StartDate = state.StartDate,
        EndDate = state.EndDate
    };

    public static TravelPlanState ToDto(TravelPlanEntity entity) => new()
    {
        Origin = entity.Origin,
        Destination = entity.Destination,
        NumberOfTravelers = entity.NumberOfTravelers,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate
    };
}
