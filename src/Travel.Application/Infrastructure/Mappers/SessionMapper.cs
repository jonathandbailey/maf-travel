using Travel.Application.Api.Infrastructure.Documents;
using Travel.Application.Domain;

namespace Travel.Application.Infrastructure.Mappers;

public static class SessionMapper
{
    public static SessionDocument ToDocument(this Session session)
    {
        return new SessionDocument
        {
            ThreadId = session.ThreadId,
            TravelPlanId = session.TravelPlanId
        };
    }

    public static Session ToDomain(this SessionDocument document)
    {
        var session = new Session(document.ThreadId, document.TravelPlanId);
        return session;
    }
}
