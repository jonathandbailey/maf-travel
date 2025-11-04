namespace Application.Workflows.Conversations.Dto;

public record NextActionParameters(
    string[] Questions,
    Slot[] Slots
);
