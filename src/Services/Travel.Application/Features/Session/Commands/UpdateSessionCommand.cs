using MediatR;
using Travel.Application.Interfaces;

namespace Travel.Application.Features.Session.Commands;

public record UpdateSessionCommand(Guid SessionId, Guid TravelPlanId) : IRequest<SessionResponse>;

public class UpdateSessionCommandHandler(
    ISessionRepository repository) : IRequestHandler<UpdateSessionCommand, SessionResponse>
{
    public async Task<SessionResponse> Handle(UpdateSessionCommand command, CancellationToken cancellationToken)
    {
        var session = await repository.GetAsync(command.SessionId, cancellationToken);
        var updated = session with { TravelPlanId = command.TravelPlanId };

        await repository.UpdateAsync(updated, cancellationToken);

        return new SessionResponse(updated.Id, updated.CreatedAt, updated.TravelPlanId);
    }
}
