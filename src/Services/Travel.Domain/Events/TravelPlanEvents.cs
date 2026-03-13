using MediatR;

namespace Travel.Domain.Events;

public record TravelPlanCreatedEvent(Guid Id) : INotification;

public record TravelPlanUpdatedEvent(Guid Id) : INotification;
