using Infrastructure.Repository.Entities;
using Travel.Agents.Dto;

namespace Travel.Agents.Mappers;

public static class TravelPlanMapper
{
    public static TravelPlanEntity ToEntity(Guid planId, TravelPlanDto dto) => new()
    {
        Id = planId,
        Origin = dto.Origin,
        Destination = dto.Destination,
        NumberOfTravelers = dto.NumberOfTravelers,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate
    };

    public static TravelPlanDto ToDto(TravelPlanEntity entity) => new()
    {
        Origin = entity.Origin,
        Destination = entity.Destination,
        NumberOfTravelers = entity.NumberOfTravelers,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate
    };
}
