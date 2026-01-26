using System.Text.Json;
using Infrastructure.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Travel.Application.Api.Domain;
using Travel.Application.Api.Infrastructure.Documents;
using Travel.Application.Api.Infrastructure.Mappers;

namespace Travel.Application.Api.Infrastructure;

public class SessionRepository(IAzureStorageRepository azureStorageRepository, IOptions<AzureStorageSettings> settings) : ISessionRepository
{
    private const string ApplicationJsonContentType = "application/json";

    public async Task SaveAsync(Guid userId, Session session)
    {
        var serializedSession = JsonSerializer.Serialize(session.ToDocument());

        await azureStorageRepository.UploadTextBlobAsync
            (GetResource(userId, session.ThreadId), settings.Value.ContainerName, serializedSession, ApplicationJsonContentType);
    }

    public async Task<Session> LoadAsync(Guid userId, Guid sessionId)
    {
        var payload = await azureStorageRepository.DownloadTextBlobAsync(GetResource(userId, sessionId), settings.Value.ContainerName);

        var session = JsonSerializer.Deserialize<SessionDocument>(payload);

        if (session == null)
            throw new ArgumentException("Session not found");

        return session.ToDomain();
    }

    private string GetResource(Guid userId, Guid threadId)
    {
        return $"{userId}/sessions/{threadId}.json";
    }
}

public interface ISessionRepository
{
    Task SaveAsync(Guid userId, Session session);
    Task<Session> LoadAsync(Guid userId, Guid sessionId);
}