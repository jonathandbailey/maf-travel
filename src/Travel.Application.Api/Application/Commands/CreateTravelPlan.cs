using MediatR;
using Travel.Application.Api.Domain;
using Travel.Application.Api.Infrastructure;

namespace Travel.Application.Api.Application.Commands;

public record CreateTravelPlanCommand(Guid UserId) : IRequest<Guid>;

public class CreateTravelPlanCommandHandler(ITravelPlanRepository travelPlanRepository) : IRequestHandler<CreateTravelPlanCommand, Guid>
{
    public async Task<Guid> Handle(CreateTravelPlanCommand request, CancellationToken cancellationToken)
    {
        var travelPlan = new TravelPlan();
        
        await travelPlanRepository.SaveAsync(travelPlan, request.UserId);

        return travelPlan.Id;
    }
}