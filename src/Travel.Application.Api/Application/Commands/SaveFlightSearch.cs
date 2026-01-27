using MediatR;
using Travel.Application.Api.Domain.Flights;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Infrastructure;
using Travel.Application.Api.Infrastructure.Mappers;

namespace Travel.Application.Api.Application.Commands;

public record SaveFlightSearchCommand(Guid UserId, Guid SessionId, FlightSearchResultDto FlightSearchResult) : IRequest<Guid>;

public class SaveFlightSearchCommandHandler(ITravelPlanRepository travelPlanRepository, IFlightRepository flightRepository, ISessionRepository sessionRepository) :
    IRequestHandler<SaveFlightSearchCommand, Guid>
{
    public async Task<Guid> Handle(SaveFlightSearchCommand request, CancellationToken cancellationToken)
    {
        var flightSearch = request.FlightSearchResult.ToDomain();
        var id = await flightRepository.SaveFlightSearch(flightSearch);

        var session = await sessionRepository.LoadAsync(request.UserId, request.SessionId);

        var travelPlan = await travelPlanRepository.LoadAsync(request.UserId, session.TravelPlanId);

        travelPlan.AddFlightSearchOption(new FlightOptionSearch(id));

        await travelPlanRepository.SaveAsync(travelPlan, request.UserId);

        return id;
    }
}