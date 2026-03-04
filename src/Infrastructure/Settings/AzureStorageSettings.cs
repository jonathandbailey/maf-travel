namespace Infrastructure.Settings;

public class AzureStorageSettings
{
    public required string AgentThreadContainerName { get; init; }
    public required string CheckpointContainerName { get; init; }
}
