namespace Application.Workflows.Conversations.Dto;

public record Slot(
    string Name,
    string Capability,
    string Reason
);
