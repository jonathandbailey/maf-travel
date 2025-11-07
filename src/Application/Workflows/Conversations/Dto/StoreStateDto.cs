using System.Text.Json;
using Microsoft.Agents.AI.Workflows;

namespace Application.Workflows.Conversations.Dto;

public class StoreStateDto(CheckpointInfo checkpointInfo, JsonElement jsonElement)
{
    public CheckpointInfo CheckpointInfo { get; init; } = checkpointInfo;

    public JsonElement JsonElement { get; init; } = jsonElement;
}