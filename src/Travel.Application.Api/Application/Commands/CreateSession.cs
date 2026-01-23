using MediatR;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Models;
using Travel.Application.Api.Services;

namespace Travel.Application.Api.Application.Commands;

public record CreateSessionCommand(Guid UserId) : IRequest<SessionDto>;

public class CreateSessionCommandHandler(ISessionService sessionService, ITravelPlanRepository travelPlanRepository) : IRequestHandler<CreateSessionCommand, SessionDto>
{
    public async Task<SessionDto> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var travelPlan = new TravelPlan();
        
        await travelPlanRepository.SaveAsync(travelPlan, request.UserId);

        var session = await sessionService.Create(request.UserId, travelPlan.Id);
        return new SessionDto(session.ThreadId, session.TravelPlanId);
    }
}