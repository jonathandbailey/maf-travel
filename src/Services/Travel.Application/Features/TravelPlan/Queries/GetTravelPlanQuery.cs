using MediatR;
using Travel.Application.Interfaces;

namespace Travel.Application.Features.TravelPlan.Queries;

public record GetTravelPlanQuery(Guid Id) : IRequest<TravelPlanResponse>;

public class GetTravelPlanQueryHandler(ITravelPlanQuery travelPlanQuery, ISessionRepository sessionRepository)
    : IRequestHandler<GetTravelPlanQuery, TravelPlanResponse>
{
    public async Task<TravelPlanResponse> Handle(GetTravelPlanQuery query, CancellationToken cancellationToken)
    {
        var plan = await travelPlanQuery.GetAsync(query.Id, cancellationToken);
        
        var session = await sessionRepository.GetByTravelPlanIdAsync(plan.Id, cancellationToken);
        
        return new TravelPlanResponse(plan.Id, plan.Origin, plan.Destination, plan.NumberOfTravelers, plan.StartDate, plan.EndDate, session?.Id);
    }
}
