namespace Application.Workflows.ReAct.Dto;

public class ActObservation(string message)
{
    public string Message { get; init; } = message;
}