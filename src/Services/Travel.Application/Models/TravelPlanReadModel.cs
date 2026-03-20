namespace Travel.Application.Models;

public record TravelPlanReadModel(
    Guid Id,
    string? Origin,
    string? Destination,
    int? NumberOfTravelers,
    DateTime? StartDate,
    DateTime? EndDate);
