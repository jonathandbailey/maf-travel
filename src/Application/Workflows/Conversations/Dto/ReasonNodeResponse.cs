namespace Application.Workflows.Conversations.Dto;

public record ReasonNodeResponse(
    string ChosenCapability,
    string[] MissingInputs,
    string Rationale,
    NextAction NextAction
);
