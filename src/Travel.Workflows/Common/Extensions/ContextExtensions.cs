using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Dto;



namespace Travel.Workflows.Common.Extensions;

public static class ContextExtensions
{
    private const string TravelPlanKey = "TravelPlan";
    private const string? TravelPlanScopeName = "TravelPlanScope";

    private const string ThreadIdKey = "ThreadIdKey";
    private const string? ThreadIdScopeName = "ThreadIdScope";

    public static async Task<TravelPlanState> GetTravelPlan(this IWorkflowContext context, CancellationToken cancellationToken)
    {
        var travelPlan = await context.ReadStateAsync<TravelPlanState>(TravelPlanKey, scopeName: TravelPlanScopeName, cancellationToken: cancellationToken)
                         ?? throw new InvalidOperationException("Travel Plan State Not Found.");

        return travelPlan;
    }

    public static async Task SetTravelPlan(this IWorkflowContext context, TravelPlanState travelPlan, CancellationToken cancellationToken)
    {
        await context.QueueStateUpdateAsync(TravelPlanKey, travelPlan, scopeName: TravelPlanScopeName, cancellationToken: cancellationToken);
    }

    public static async Task<Guid> GetThreadId(this IWorkflowContext context, CancellationToken cancellationToken)
    {
        var threadId = await context.ReadStateAsync<Guid>(ThreadIdKey, scopeName: ThreadIdScopeName, cancellationToken: cancellationToken);

        if (threadId == Guid.Empty)
        {
            throw new InvalidOperationException("Thread ID State Not Found.");
        }

        return threadId;
    }

    public static async Task SetThreadId(this IWorkflowContext context, Guid threadId, CancellationToken cancellationToken)
    {
        await context.QueueStateUpdateAsync(ThreadIdKey, threadId, scopeName: ThreadIdScopeName, cancellationToken: cancellationToken);
    }
}