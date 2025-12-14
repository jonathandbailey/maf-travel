using Application.Agents;
using Application.Observability;
using Application.Workflows.Dto;
using Application.Workflows.Events;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Workflows.Nodes;

public class FlightWorkerNode(IAgent agent) : 
    ReflectingExecutor<FlightWorkerNode>(WorkflowConstants.FlightWorkerNodeName), 
   
    IMessageHandler<CreateFlightOptions>
{
    private const string FlightWorkerNodeError = "Flight Worker Node has failed to execute.";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async ValueTask HandleAsync(CreateFlightOptions message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.FlightWorkerNodeName}.handleRequest");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.FlightWorkerNodeName);

        try
        {
            var serialized = JsonSerializer.Serialize(message);

            WorkflowTelemetryTags.SetInputPreview(activity, serialized);

            var response = await agent.RunAsync(new ChatMessage(ChatRole.User, serialized), cancellationToken: cancellationToken);

            var responseMessage = response.Messages.First();

            WorkflowTelemetryTags.SetInputPreview(activity, responseMessage.Text);

            activity?.SetTag(WorkflowTelemetryTags.ArtifactKey, "flights");

            var flightOptions = JsonSerializer.Deserialize<FlightActionResultDto>(responseMessage.Text, SerializerOptions);

            if (flightOptions == null)
                throw new JsonException("Failed to deserialize flight options in Flight Worker Node");

            var payload = JsonSerializer.Serialize(flightOptions.FlightOptions, SerializerOptions);

            await context.SendMessageAsync(new ArtifactStorageDto("flights", payload), cancellationToken: cancellationToken);

            await context.SendMessageAsync(new FlightOptionsCreated(), cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(FlightWorkerNodeError, "flights", WorkflowConstants.FlightWorkerNodeName, exception), cancellationToken);
        }
    }
}