using Application.Agents;
using Application.Observability;
using Application.Workflows.Dto;
using Application.Workflows.Events;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Application.Workflows.Nodes;

public class TrainWorkerNode(IAgent agent) : ReflectingExecutor<TrainWorkerNode>(WorkflowConstants.TrainWorkerNodeName), IMessageHandler<OrchestratorWorkerTaskDto>
{
    private const string TrainWorkerNodeError = "Train Worker Node has failed to execute.";
    public async ValueTask HandleAsync(OrchestratorWorkerTaskDto message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.TrainWorkerNodeName}.handleRequest");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.TrainWorkerNodeName);

        try
        {
            var serialized = JsonSerializer.Serialize(message);

            WorkflowTelemetryTags.SetInputPreview(activity, serialized);
    
            var response = await agent.RunAsync(new ChatMessage(ChatRole.User, serialized), cancellationToken: cancellationToken);
    
            WorkflowTelemetryTags.SetInputPreview(activity, response.Text);

            activity?.SetTag(WorkflowTelemetryTags.ArtifactKey, message.ArtifactKey);

            await context.SendMessageAsync(new ArtifactStorageDto(message.ArtifactKey, response.Text), cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(TrainWorkerNodeError, message.ArtifactKey, WorkflowConstants.TrainWorkerNodeName, exception), cancellationToken);
        }
    }
}