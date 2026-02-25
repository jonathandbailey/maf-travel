using Microsoft.Agents.AI.Workflows;

namespace Travel.Workflows.Common;

public record WorkflowSession(
    Guid ThreadId,
    WorkflowState State,
    CheckpointInfo? LastCheckpoint);
