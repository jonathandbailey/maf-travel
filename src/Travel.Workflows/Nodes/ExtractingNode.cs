using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Travel.Workflows.Dto;

namespace Travel.Workflows.Nodes;

public class ExtractingNode(AIAgent agent) : ReflectingExecutor<ExtractingNode>("Extracting"), IMessageHandler<ChatMessage>
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async ValueTask HandleAsync(ChatMessage message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var response = await agent.RunAsync(message,  cancellationToken: cancellationToken);

        foreach (var chatMessage in response.Messages)
        {
            foreach (var content in chatMessage.Contents)
            {
                if (content is FunctionCallContent functionCall)
                {
                    if (functionCall.Arguments != null && functionCall.Arguments.ContainsKey("travelPlan"))
                    {
                        var argumentsJson = JsonSerializer.Serialize(functionCall.Arguments["travelPlan"]);

                        var details = JsonSerializer.Deserialize<TravelPlanDto>(argumentsJson, _serializerOptions);

                        if (details != null)
                        {
                            await context.SendMessageAsync(new TravelPlanUpdateCommand(details), cancellationToken: cancellationToken);
                        }
                    }
                }
            }
        }
    }
}