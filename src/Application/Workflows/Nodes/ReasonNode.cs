using Application.Agents;
using Application.Observability;
using Application.Services;
using Application.Workflows.Dto;
using Application.Workflows.Events;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Workflows.Nodes;

public class ReasonNode(IAgent agent, ITravelPlanService travelPlanService) : ReflectingExecutor<ReasonNode>(WorkflowConstants.ReasonNodeName),
   
    IMessageHandler<ReasoningInputDto, ReasoningOutputDto>
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async ValueTask<ReasoningOutputDto> HandleAsync(
        ReasoningInputDto actObservation, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.ReasonNodeName}.observe");
 
        var message = await Create(context, actObservation);
      
        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.InputNodePreview, message.Text);

        var actRequest = await RunReasoningAsync(message, context, activity, cancellationToken);

        return actRequest;
    }

    private async Task<ChatMessage> Create(IWorkflowContext context, ReasoningInputDto reasoningInput)
    {

        var travelPlanSummary = await travelPlanService.GetSummary();

        var serialized = JsonSerializer.Serialize(reasoningInput, SerializerOptions);

        var template = $"Observation :{reasoningInput.Observation}\nTravelPlanSummary :{travelPlanSummary}";

        return new ChatMessage(ChatRole.User, template);
    }

    private async Task<ReasoningOutputDto> RunReasoningAsync(ChatMessage message, IWorkflowContext context, Activity? activity,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await agent.RunAsync(message, cancellationToken);

            WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.OutputNodePreview, response.Text);

            var actRequest = response.Deserialize<ReasoningOutputDto>(JsonSerializerOptions.Web);

            await context.AddEventAsync(new WorkflowStatusEvent(actRequest.Status, actRequest.Thought, WorkflowConstants.ReasonNodeName), cancellationToken);

            return actRequest;
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent("Reason Node Error", "Reason Node", WorkflowConstants.ReasonNodeName, exception), cancellationToken);
            throw;
        }
    }

}