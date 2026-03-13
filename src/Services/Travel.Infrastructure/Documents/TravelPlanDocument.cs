namespace Travel.Infrastructure.Documents;

internal record TravelPlanDocument(
    Guid Id,
    string? Origin,
    string? Destination,
    int? NumberOfTravelers,
    DateTime? StartDate,
    DateTime? EndDate);
