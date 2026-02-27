namespace Infrastructure.Settings;

public class FileStorageSettings
{
    public string? RootFolder { get; init; }

    public required string AgentTemplateFolder { get; init; }

    public required string AgentThreadFolder { get; init; }

    public required string CheckpointFolder { get; init; }

    public required string SessionFolder { get; init; }

    public string ResolveFolder(string subFolder)
    {
        if (Path.IsPathRooted(subFolder))
            return subFolder;

        var baseDir = string.IsNullOrEmpty(RootFolder)
            ? AppContext.BaseDirectory
            : Path.Combine(AppContext.BaseDirectory, RootFolder);

        return Path.Combine(baseDir, subFolder);
    }
}