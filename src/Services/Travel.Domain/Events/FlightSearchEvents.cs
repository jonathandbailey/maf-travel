using MediatR;

namespace Travel.Domain.Events;

public record FlightSearchCreatedEvent(Guid Id) : INotification;
