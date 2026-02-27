using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Repository;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Travel.Workflows.Common;

namespace Travel.Workflows.Infrastructure;

public interface IWorkflowSessionRepository
{
    Task<WorkflowSession?> LoadAsync(Guid threadId);
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

    public async Task<WorkflowSession?> LoadAsync(Guid threadId)
    {
        var path = BuildPath($"{threadId}-session.json");
        try
        {
            var content = await fileRepository.LoadAsync(path);
            return JsonSerializer.Deserialize<WorkflowSession>(content, SerializerOptions);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
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
