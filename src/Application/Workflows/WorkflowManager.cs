using Application.Infrastructure;
using Application.Observability;
using Application.Workflows.Conversations;
using Application.Workflows.Conversations.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Workflows;

public class WorkflowManager(IAzureStorageRepository repository, IOptions<AzureStorageSeedSettings> settings) : IWorkflowManager
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private ConversationCheckpointStore _checkpointStore;
    private Guid _sessionId;
    public CheckpointManager CheckpointManager { get; private set; }

    public CheckpointInfo CheckpointInfo { get; set; }

    public WorkflowState State { get; private set; } = WorkflowState.Initialized;

    public async Task<WorkflowStateDto> Initialize(Guid sessionId)
    {
        using var activity = Telemetry.StarActivity("WorkflowManager-[initialize]");

        _sessionId = sessionId;

        var stateDto = await GetOrCreateCheckpointStore(_sessionId);

        CheckpointManager = CheckpointManager.CreateJson(_checkpointStore);

        return stateDto;
    }
   

    public async Task Save(WorkflowState state, CheckpointInfo checkpointInfo)
    {
        using var activity = Telemetry.StarActivity("WorkflowManager-[save]");

        var workflowStateDto = new WorkflowStateDto(state, checkpointInfo);
        
        var serializedConversation = JsonSerializer.Serialize(workflowStateDto, SerializerOptions);

        await repository.UploadTextBlobAsync($"{_sessionId}.json", settings.Value.ContainerName,
            serializedConversation, ApplicationJsonContentType);
    }

    private async Task<WorkflowStateDto> GetOrCreateCheckpointStore(Guid sessionId)
    {
        var blobExists = await repository.BlobExists($"{sessionId}.json", settings.Value.ContainerName);

        if (blobExists == false)
        {
            State = WorkflowState.Initialized;

            return new WorkflowStateDto(WorkflowState.Initialized, null);
        }

        var blob = await repository.DownloadTextBlobAsync($"{sessionId}.json", settings.Value.ContainerName);

        var stateDto = JsonSerializer.Deserialize<WorkflowStateDto>(blob, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Checkpoint Store for session : {sessionId}");

        State = stateDto.State;

        CheckpointInfo = stateDto.CheckpointInfo;

        return stateDto;
    }
}

public interface IWorkflowManager
{
    Task Save(WorkflowState state, CheckpointInfo checkpointInfo);
   
    Task<WorkflowStateDto> Initialize(Guid sessionId);
}