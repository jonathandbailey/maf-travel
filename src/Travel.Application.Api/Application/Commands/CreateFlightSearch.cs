using Infrastructure.Dto;
using MediatR;
using Travel.Application.Api.Services;

namespace Travel.Application.Api.Application.Commands;

public record CreateFlightSearchCommand(Guid UserId, Guid SessionId, FlightSearchDto FlightSearch) : IRequest<Guid>;

public class CreateFlightSearchCommandHandler(ITravelPlanService travelPlanService, IFlightService flightService, ISessionService sessionService) :
    IRequestHandler<CreateFlightSearchCommand, Guid>
{
    public async Task<Guid> Handle(CreateFlightSearchCommand request, CancellationToken cancellationToken)
    {
        var id = await flightService.SaveFlightSearch(request.FlightSearch);

        var session = await sessionService.Get(request.UserId, request.SessionId);

        await travelPlanService.UpdateFlightSearchOption(request.UserId, session.TravelPlanId, id);

        return id;
    }
}