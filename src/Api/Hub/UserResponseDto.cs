namespace Api.Hub;

public class UserResponseDto
{
    public string Message { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public bool IsEndOfStream { get; set; } = false;
}

public record StatusDto(string Message, string Details, Guid RequestId);