using Microsoft.Extensions.AI;

namespace Application.Workflows.ReAct.Dto;

public class ActRequest(ChatMessage message)
{
    public ChatMessage Message { get; init; } = message;
}