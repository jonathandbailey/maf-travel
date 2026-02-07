using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Travel.Workflows.Planning.Dto;
using Travel.Workflows.Planning.Services;

namespace Travel.Workflows.Planning.Nodes;

public class TravelPlanNode(ITravelPlanService travelPlanService) : ReflectingExecutor<TravelPlanNode>("TravelPlan"), IMessageHandler<FunctionCallContent, ChatMessage>
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async ValueTask<ChatMessage> HandleAsync(FunctionCallContent functionCallContent, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var argumentsJson = JsonSerializer.Serialize(functionCallContent.Arguments["informationRequest"]);

        var details = JsonSerializer.Deserialize<TravelPlanDto>(argumentsJson, _serializerOptions);

        await travelPlanService.Update(details);


        return new ChatMessage(ChatRole.User, "Travel plan updated successfully.");
    }
}