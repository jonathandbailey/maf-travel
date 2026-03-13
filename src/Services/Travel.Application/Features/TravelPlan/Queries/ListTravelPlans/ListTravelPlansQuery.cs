using MediatR;
using Travel.Application.Interfaces;

namespace Travel.Application.Features.TravelPlan.Queries.ListTravelPlans;

public record ListTravelPlansQuery : IRequest<IReadOnlyList<TravelPlanResponse>>;

public class ListTravelPlansQueryHandler(ITravelPlanRepository repository)
    : IRequestHandler<ListTravelPlansQuery, IReadOnlyList<TravelPlanResponse>>
{
    public async Task<IReadOnlyList<TravelPlanResponse>> Handle(ListTravelPlansQuery query, CancellationToken cancellationToken)
    {
        var plans = await repository.ListAsync(cancellationToken);
        return plans
            .Select(p => new TravelPlanResponse(p.Id, p.Origin, p.Destination, p.NumberOfTravelers, p.StartDate, p.EndDate))
            .ToList();
    }
}
