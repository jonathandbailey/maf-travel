using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Travel.Workflows.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Extensions;
using Travel.Workflows.Telemetry;

namespace Travel.Workflows.Nodes;

public class ExtractionNode(AIAgent agent) : ReflectingExecutor<ExtractionNode>("Extraction"), IMessageHandler<ChatMessage>
{
    public async ValueTask HandleAsync(ChatMessage message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode("Extraction", Guid.NewGuid());

        activity?.AddNodeAgentInput(message.Text);

        var response = await agent.RunAsync(message,  cancellationToken: cancellationToken);

        activity?.AddNodeAgentOutput(response.Text);
        activity?.AddNodeAgentUsage(response);

        foreach (var responseMessage in response.Messages)
        {
            foreach (var content in responseMessage.Contents)
            {
                if (content is FunctionCallContent functionCallContent)
                {
                    using var toolCallActivity = TravelWorkflowTelemetry.ToolCall(functionCallContent.Name, functionCallContent.Arguments, activity);
                }
            }
        }

        if (response.TryGetFunctionArgument<TravelPlanDto>(WorkflowConstants.ExtractingNodeUpdatePlanFunctionName, out var details, Json.FunctionCallSerializerOptions))
        {
            await context.SendMessageAsync(new TravelPlanUpdateCommand(details), cancellationToken);
        }
    }
}