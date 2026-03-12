using MediatR;
using Travel.Application.Interfaces;

namespace Travel.Application.Features.TravelPlan.Delete;

public record DeleteTravelPlanCommand(Guid Id) : IRequest;

public class DeleteTravelPlanCommandHandler(ITravelPlanRepository repository)
    : IRequestHandler<DeleteTravelPlanCommand>
{
    public Task Handle(DeleteTravelPlanCommand command, CancellationToken cancellationToken)
        => repository.DeleteAsync(command.Id, cancellationToken);
}
