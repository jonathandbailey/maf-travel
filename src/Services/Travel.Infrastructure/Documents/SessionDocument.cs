namespace Travel.Infrastructure.Documents;

internal record SessionDocument(Guid Id, DateTime CreatedAt, Guid? TravelPlanId);
