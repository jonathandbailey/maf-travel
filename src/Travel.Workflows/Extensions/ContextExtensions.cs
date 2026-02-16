using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Dto;



namespace Travel.Workflows.Extensions;

public static class ContextExtensions
{
    private const string TravelPlanKey = "TravelPlan";
    private const string? TravelPlanScopeName = "TravelPlanScope";

    public static async Task<TravelPlanDto> GetTravelPlan(this IWorkflowContext context, CancellationToken cancellationToken)
    {
        var travelPlan = await context.ReadStateAsync<TravelPlanDto>(TravelPlanKey, scopeName: TravelPlanScopeName, cancellationToken: cancellationToken)
                         ?? throw new InvalidOperationException("Travel Plan State Not Found.");

        return travelPlan;
    }

    public static async Task SetTravelPlan(this IWorkflowContext context, TravelPlanDto travelPlan, CancellationToken cancellationToken)
    {
        await context.QueueStateUpdateAsync(TravelPlanKey, travelPlan, scopeName: TravelPlanScopeName, cancellationToken: cancellationToken);
    }
}