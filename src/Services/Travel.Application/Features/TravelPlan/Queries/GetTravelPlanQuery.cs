using MediatR;
using Travel.Application.Interfaces;

namespace Travel.Application.Features.TravelPlan.Queries;

public record GetTravelPlanQuery(Guid Id) : IRequest<TravelPlanResponse>;

public class GetTravelPlanQueryHandler(ITravelPlanRepository repository, ISessionRepository sessionRepository)
    : IRequestHandler<GetTravelPlanQuery, TravelPlanResponse>
{
    public async Task<TravelPlanResponse> Handle(GetTravelPlanQuery query, CancellationToken cancellationToken)
    {
        var plan = await repository.GetAsync(query.Id, cancellationToken);
        var session = await sessionRepository.GetByTravelPlanIdAsync(plan.Id, cancellationToken);
        return new TravelPlanResponse(plan.Id, plan.Origin, plan.Destination, plan.NumberOfTravelers, plan.StartDate, plan.EndDate, session?.Id);
    }
}
