using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using System.Text.Json;
using Application.Infrastructure;
using Application.Workflows.ReAct.Dto;

namespace Application.Workflows;

public class CheckpointStore(ICheckpointRepository checkpointRepository) : JsonCheckpointStore
{
    public override async ValueTask<IEnumerable<CheckpointInfo>> RetrieveIndexAsync(string runId, CheckpointInfo? withParent = null)
    {
        var stateByRunId = await checkpointRepository.GetAsync(runId);

        return stateByRunId.Select(x => x.CheckpointInfo);
    }

    public override async ValueTask<CheckpointInfo> CreateCheckpointAsync(string runId, JsonElement value, CheckpointInfo? parent = null)
    {
        var checkpointInfo = new CheckpointInfo(runId, Guid.NewGuid().ToString());

        await checkpointRepository.SaveAsync(new StoreStateDto(checkpointInfo, value));
       
        return checkpointInfo;
    }

    public override async ValueTask<JsonElement> RetrieveCheckpointAsync(string runId, CheckpointInfo key)
    {
        var stateDto = await checkpointRepository.LoadAsync(key.CheckpointId, runId);

        return stateDto.JsonElement;
    }
}