namespace Application.Workflows.ReWoo.Dto;

public class OrchestrationRequest(string text)
{
    public string Text { get; } = text;
}