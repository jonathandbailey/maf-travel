namespace Travel.Application.Features.TravelPlan;

public record TravelPlanResponse(
    Guid Id,
    string? Origin,
    string? Destination,
    int? NumberOfTravelers,
    DateTime? StartDate,
    DateTime? EndDate,
    Guid? SessionId = null);
