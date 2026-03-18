using System.Runtime.CompilerServices;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using Travel.Agents.Services;
using Travel.Workflows.Common.Interfaces;
using Travel.Workflows.Flights.Dto;
using Travel.Workflows.Flights.Events;

namespace Travel.Workflows.Flights.Services;

public class FlightsWorkflowService(
    IAgentProvider agentProvider,
    McpClient mcpClient,
    IFlightApiClient flightApiClient,
    ITravelApiClient travelApiClient,
    ILogger<FlightsWorkflowService> logger)
{
    public async IAsyncEnumerable<WorkflowEvent> SearchAsync(
        FlightsWorkflowRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var plan = await travelApiClient.GetPlanBySessionAsync(request.SessionId, cancellationToken);

        var missingFields = new List<string>();
        if (string.IsNullOrWhiteSpace(plan.Origin)) missingFields.Add(nameof(plan.Origin));
        if (string.IsNullOrWhiteSpace(plan.Destination)) missingFields.Add(nameof(plan.Destination));
        if (!plan.StartDate.HasValue) missingFields.Add(nameof(plan.StartDate));

        if (missingFields.Count > 0)
        {
            yield return new FlightPlanValidationFailedEvent(
                $"Cannot search for flights. The following required fields are not populated on the travel plan: {string.Join(", ", missingFields)}.");
            yield break;
        }

        var input = new FlightsWorkflowInput(
            plan.Origin!,
            plan.Destination!,
            DateOnly.FromDateTime(plan.StartDate!.Value),
            plan.EndDate.HasValue ? DateOnly.FromDateTime(plan.EndDate.Value) : null,
            plan.NumberOfTravelers ?? 1);

        var flightAgent = await agentProvider.CreateAsync(AgentType.Flight);
        var workflow = FlightsWorkflowBuilder.Build(flightAgent, mcpClient);
        var checkpointManager = CheckpointManager.Default;

        var run = await InProcessExecution.RunStreamingAsync(workflow, input, checkpointManager);

        await foreach (var evt in run.WatchStreamAsync(cancellationToken))
        {
            yield return evt;

            if (evt is FlightSearchCompleteEvent completeEvt)
            {
                var savedId = await TrySaveFlightSearchAsync(completeEvt.Flights, cancellationToken);
                if (savedId.HasValue)
                    yield return new FlightSearchSavedEvent(savedId.Value);
            }
        }
    }

    private async Task<Guid?> TrySaveFlightSearchAsync(IReadOnlyList<FlightOption> flights, CancellationToken cancellationToken)
    {
        try
        {
            return await flightApiClient.CreateFlightSearchAsync(flights, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to save flight search results.");
            return null;
        }
    }
}
