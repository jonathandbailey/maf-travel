namespace Infrastructure.Settings;

public class FileStorageSettings
{
    public required string AgentTemplateFolder { get; init; }

    public required string AgentThreadFolder { get; init; }
}