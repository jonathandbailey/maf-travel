using Microsoft.Extensions.AI;

namespace Application.Workflows.Dto;

public class TravelWorkflowRequestDto(ChatMessage message, Guid userId, Guid sessionId, Guid exchangeId)
{
    public Guid SessionId { get; } = sessionId;

    public Guid UserId { get; } = userId;

    public Guid RequestId { get; } = exchangeId;

    public ChatMessage Message { get; } = message;
}