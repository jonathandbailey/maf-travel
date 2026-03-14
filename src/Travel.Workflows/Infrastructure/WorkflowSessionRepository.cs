using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Repository;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Travel.Workflows.Common;

namespace Travel.Workflows.Infrastructure;

public interface IWorkflowSessionRepository
{
    Task<bool> ExistsAsync(Guid threadId);
    Task<WorkflowSession> LoadAsync(Guid threadId);
    Task SaveAsync(WorkflowSession session);
}

public class WorkflowSessionRepository(IFileRepository fileRepository, IOptions<FileStorageSettings> settings) : IWorkflowSessionRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public Task<bool> ExistsAsync(Guid threadId) =>
        fileRepository.ExistsAsync(BuildPath($"{threadId}-session.json"));

    public async Task<WorkflowSession> LoadAsync(Guid threadId)
    {
        var path = BuildPath($"{threadId}-session.json");
        var content = await fileRepository.LoadAsync(path);
        var deserialized = JsonSerializer.Deserialize<WorkflowSession>(content, SerializerOptions);

        return deserialized ?? throw new JsonException("Failed to deserialize WorkflowSession.");
    }

    public async Task SaveAsync(WorkflowSession session)
    {
        var path = BuildPath($"{session.ThreadId}-session.json");
        var content = JsonSerializer.Serialize(session, SerializerOptions);
        await fileRepository.SaveAsync(path, content);
    }

    private string BuildPath(string fileName) =>
        Path.Combine(settings.Value.ResolveFolder(settings.Value.SessionFolder), fileName);
}
