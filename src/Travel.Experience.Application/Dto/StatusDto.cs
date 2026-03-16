namespace Travel.Experience.Application.Dto;

public class SnapShot<T>(string type, T payload)
{
    public string Type { get; } = type;
    public T Payload { get; } = payload;
}

public class StatusUpdate(string type, string source, string status, string details)
{
    public string Type { get; } = type;

    public string Source { get; } = source;

    public string Status { get; } = status;

    public string Details { get; } = details;
}

public record ArtifactCreated(string ArtifactType, Guid Id);
