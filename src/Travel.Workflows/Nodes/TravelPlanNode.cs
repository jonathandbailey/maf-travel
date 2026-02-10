using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Services;

namespace Travel.Workflows.Nodes;

public class TravelPlanNode(ITravelPlanService travelPlanService) : ReflectingExecutor<TravelPlanNode>("TravelPlan"), IMessageHandler<FunctionCallContent, TravelPlanDto>
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async ValueTask<TravelPlanDto> HandleAsync(FunctionCallContent functionCallContent, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var argumentsJson = JsonSerializer.Serialize(functionCallContent.Arguments["travelPlan"]);

        var details = JsonSerializer.Deserialize<TravelPlanDto>(argumentsJson, _serializerOptions);

        await travelPlanService.Update(details);

        await context.AddEventAsync(new TravelPlanUpdateEvent(details), cancellationToken);

        return details;
    }
}