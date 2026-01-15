using Microsoft.Extensions.AI;

namespace Workflows.Dto;

public class TravelWorkflowRequestDto(ChatMessage message)
{
    public ChatMessage Message { get; } = message;
}