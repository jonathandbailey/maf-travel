using Infrastructure.Interfaces;
using Travel.Application.Api.Domain;
using Travel.Application.Api.Infrastructure.Documents;
using Travel.Application.Api.Infrastructure.Mappers;

namespace Travel.Application.Api.Infrastructure;

public class SessionRepository(IArtifactRepository artifactRepository) : ISessionRepository
{
    public async Task SaveAsync(Guid userId, Session session)
    {
        await artifactRepository.SaveAsync(session.ToDocument(), session.ThreadId.ToString(), GetResourceContainer(userId));
    }

    public async Task<Session> LoadAsync(Guid userId, Guid sessionId)
    {

        var sessionEx = await artifactRepository.LoadAsyncEx<SessionDocument>(sessionId.ToString(), GetResourceContainer(userId));


        return sessionEx.ToDomain();
    }

    private static string GetResourceContainer(Guid userId)
    {
        return $"{userId}/sessions/";
    }
}

public interface ISessionRepository
{
    Task SaveAsync(Guid userId, Session session);
    Task<Session> LoadAsync(Guid userId, Guid sessionId);
}