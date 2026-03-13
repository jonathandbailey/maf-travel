using MediatR;
using Travel.Application.Interfaces;
using SessionModel = Travel.Application.Models.Session;
using TravelPlanAggregate = Travel.Domain.Aggregates.TravelPlan;

namespace Travel.Application.Features.TravelPlan.Commands;

public record CreateTravelPlanCommand(
    string? Origin,
    string? Destination,
    int? NumberOfTravelers,
    DateTime? StartDate,
    DateTime? EndDate) : IRequest<TravelPlanResponse>;

public class CreateTravelPlanCommandHandler(
    ITravelPlanRepository repository,
    ISessionRepository sessionRepository,
    IPublisher publisher) : IRequestHandler<CreateTravelPlanCommand, TravelPlanResponse>
{
    public async Task<TravelPlanResponse> Handle(CreateTravelPlanCommand command, CancellationToken cancellationToken)
    {
        var plan = TravelPlanAggregate.Create(
            command.Origin,
            command.Destination,
            command.NumberOfTravelers,
            command.StartDate,
            command.EndDate);

        await repository.AddAsync(plan, cancellationToken);

        var session = new SessionModel(Guid.NewGuid(), DateTime.UtcNow, plan.Id);
        await sessionRepository.AddAsync(session, cancellationToken);

        foreach (var domainEvent in plan.DomainEvents)
            await publisher.Publish(domainEvent, cancellationToken);

        plan.ClearDomainEvents();

        return new TravelPlanResponse(plan.Id, plan.Origin, plan.Destination, plan.NumberOfTravelers, plan.StartDate, plan.EndDate, session.Id);
    }
}
