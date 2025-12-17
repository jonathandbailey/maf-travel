using Application.Agents;
using Application.Observability;
using Application.Services;
using Application.Workflows.Dto;
using Application.Workflows.Events;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Application.Workflows.Nodes;

public class ReasonNode(IAgent agent, ITravelPlanService travelPlanService) : ReflectingExecutor<ReasonNode>(WorkflowConstants.ReasonNodeName),
   
    IMessageHandler<ReasoningInputDto, ReasoningOutputDto>
{
    private const string NextActionError = "Error";
    private const string ReasonNodeError = "Reason Node Error";

    public async ValueTask<ReasoningOutputDto> HandleAsync(
        ReasoningInputDto reasoningInput, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.ReasonNodeName}{WorkflowConstants.Observe}");
            
        try
        {
            var travelPlanSummary = await travelPlanService.GetSummary();

            var template = JsonSerializer.Serialize(new ReasoningState(reasoningInput.Observation, travelPlanSummary));

            var message = new ChatMessage(ChatRole.User, template);

            WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.InputNodePreview, message.Text);

            var response = await agent.RunAsync(message, cancellationToken);

            WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.OutputNodePreview, response.Text);

            var reasoningOutput = response.Deserialize<ReasoningOutputDto>(JsonSerializerOptions.Web);

            await context.AddEventAsync(new WorkflowStatusEvent(reasoningOutput.Status, reasoningOutput.Thought, WorkflowConstants.ReasonNodeName), cancellationToken);

            return reasoningOutput;
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(exception.Message, ReasonNodeError, WorkflowConstants.ReasonNodeName, exception), cancellationToken);

            return new ReasoningOutputDto { NextAction = NextActionError, Status = exception.Message};
        }
    }
}