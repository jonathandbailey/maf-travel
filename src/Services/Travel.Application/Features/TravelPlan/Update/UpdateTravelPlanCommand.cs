using MediatR;
using Travel.Application.Exceptions;
using Travel.Application.Interfaces;

namespace Travel.Application.Features.TravelPlan.Update;

public record UpdateTravelPlanCommand(
    Guid Id,
    string? Origin,
    string? Destination,
    int? NumberOfTravelers,
    DateTime? StartDate,
    DateTime? EndDate) : IRequest<TravelPlanResponse>;

public class UpdateTravelPlanCommandHandler(
    ITravelPlanRepository repository,
    IPublisher publisher) : IRequestHandler<UpdateTravelPlanCommand, TravelPlanResponse>
{
    public async Task<TravelPlanResponse> Handle(UpdateTravelPlanCommand command, CancellationToken cancellationToken)
    {
        var plan = await repository.GetAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException($"TravelPlan {command.Id} not found.");

        plan.Update(
            command.Origin,
            command.Destination,
            command.NumberOfTravelers,
            command.StartDate,
            command.EndDate);

        await repository.UpdateAsync(plan, cancellationToken);

        foreach (var domainEvent in plan.DomainEvents)
            await publisher.Publish(domainEvent, cancellationToken);

        plan.ClearDomainEvents();

        return new TravelPlanResponse(plan.Id, plan.Origin, plan.Destination, plan.NumberOfTravelers, plan.StartDate, plan.EndDate);
    }
}
