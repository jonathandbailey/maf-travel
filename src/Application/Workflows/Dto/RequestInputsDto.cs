namespace Application.Workflows.Dto;

public class RequestInputsDto(string message)
{
    public string Message { get; } = message;
}