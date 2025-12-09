namespace Application.Workflows.Dto;

public class OrchestrationRequest(string text)
{
    public string Text { get; } = text;
}