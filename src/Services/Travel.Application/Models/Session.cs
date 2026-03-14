namespace Travel.Application.Models;

public record Session(Guid Id, DateTime CreatedAt, Guid? TravelPlanId);
