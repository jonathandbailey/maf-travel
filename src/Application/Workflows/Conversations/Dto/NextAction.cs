namespace Application.Workflows.Conversations.Dto;

public record NextAction(
    string Type,
    NextActionParameters Parameters
);
