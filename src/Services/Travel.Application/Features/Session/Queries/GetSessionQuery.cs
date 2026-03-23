using MediatR;
using Travel.Application.Interfaces;

namespace Travel.Application.Features.Session.Queries;

public record GetSessionQuery(Guid Id) : IRequest<SessionResponse>;

public class GetSessionQueryHandler(
    ISessionQuery sessionQuery) : IRequestHandler<GetSessionQuery, SessionResponse>
{
    public async Task<SessionResponse> Handle(GetSessionQuery query, CancellationToken cancellationToken)
    {
        var session = await sessionQuery.GetAsync(query.Id, cancellationToken);
        return new SessionResponse(session.Id, session.CreatedAt, session.TravelPlanId);
    }
}
