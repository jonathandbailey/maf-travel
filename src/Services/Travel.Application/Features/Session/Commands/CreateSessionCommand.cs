using MediatR;
using Travel.Application.Interfaces;
using SessionModel = Travel.Application.Models.Session;

namespace Travel.Application.Features.Session.Commands;

public record CreateSessionCommand() : IRequest<SessionResponse>;

public class CreateSessionCommandHandler(
    ISessionRepository repository) : IRequestHandler<CreateSessionCommand, SessionResponse>
{
    public async Task<SessionResponse> Handle(CreateSessionCommand command, CancellationToken cancellationToken)
    {
        var session = new SessionModel(Guid.NewGuid(), DateTime.UtcNow, null);

        await repository.AddAsync(session, cancellationToken);

        return new SessionResponse(session.Id, session.CreatedAt, session.TravelPlanId);
    }
}
