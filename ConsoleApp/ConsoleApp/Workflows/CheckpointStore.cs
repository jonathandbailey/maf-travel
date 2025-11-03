using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;

namespace ConsoleApp.Workflows;

public class CheckpointStore : JsonCheckpointStore
{
    private readonly Dictionary<Guid, CheckpointInfo> _checkpoints = new();
    private readonly Dictionary<CheckpointInfo, JsonElement> _checkpointElements = new();

    public void Add(Guid sessionId, CheckpointInfo checkpoint)
    {
        _checkpoints[sessionId] = checkpoint;
    }

    public CheckpointInfo Get(Guid sessionId)
    {
        return _checkpoints[sessionId];
    }

    public bool HasCheckpoint(Guid sessionId)
    {
        return _checkpoints.ContainsKey(sessionId);
    }

    public override async ValueTask<IEnumerable<CheckpointInfo>> RetrieveIndexAsync(string runId, CheckpointInfo? withParent = null)
    {
        throw new NotImplementedException();
    }

    public override ValueTask<CheckpointInfo> CreateCheckpointAsync(string runId, JsonElement value, CheckpointInfo? parent = null)
    {
        var checkpointInfo = new CheckpointInfo(runId, Guid.NewGuid().ToString());
        
        _checkpointElements.Add(checkpointInfo, value);

       return ValueTask.FromResult(checkpointInfo);
    }

    public override  ValueTask<JsonElement> RetrieveCheckpointAsync(string runId, CheckpointInfo key)
    {
        var element = _checkpointElements[key];

        return ValueTask.FromResult(element);
    }
}
