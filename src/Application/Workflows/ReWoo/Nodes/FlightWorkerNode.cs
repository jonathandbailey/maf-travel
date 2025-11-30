using Application.Agents;
using Application.Observability;
using Application.Workflows.Events;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using System.Text.Json;

namespace Application.Workflows.ReWoo.Nodes;

public class FlightWorkerNode(IAgent agent) : 
    ReflectingExecutor<FlightWorkerNode>(WorkflowConstants.FlightWorkerNodeName), 
    IMessageHandler<OrchestratorWorkerTaskDto>
{
    private const string FlightWorkerNodeError = "Flight Worker Node has failed to execute.";

    private Activity? _activity;
    
    public async ValueTask HandleAsync(OrchestratorWorkerTaskDto message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        Trace(message);

        try
        {
            var serialized = JsonSerializer.Serialize(message);
    
            var userId = await context.UserId();
            var sessionId = await context.SessionId();

            var response = await agent.RunAsync( new ChatMessage(ChatRole.User, serialized), sessionId, userId, cancellationToken: cancellationToken);
    
            var responseMessage = response.Messages.First();

            _activity?.SetTag("re-woo.output.message", response.Messages.First().Text);

            await context.SendMessageAsync(new ArtifactStorageDto(message.ArtifactKey, responseMessage.Text), cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(FlightWorkerNodeError, message.ArtifactKey, WorkflowConstants.FlightWorkerNodeName,exception), cancellationToken);
        }

        TraceEnd();
    }

    private void Trace(OrchestratorWorkerTaskDto message)
    {
        _activity = Telemetry.Start("FlightWorkerHandleRequest");

        _activity?.SetTag("re-woo.node", "flight_worker_node");

        _activity?.SetTag("re-woo.input.message", message);
    }

    private void TraceEnd()
    {
        _activity?.Dispose();
    }
}