using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tools.Registry;
using Travel.Experience.Application.Dto;
using Travel.Workflows.Flights.Dto;
using Travel.Workflows.Flights.Events;
using Travel.Workflows.Flights.Services;

namespace Travel.Experience.Application.Agents;

public sealed class FlightsWorkflowToolHandler(Func<FlightsWorkflowService> flightsWorkflowServiceFactory) : IToolHandler
{
    public const string FindFlightsToolName = "find_flights";

    private static readonly Dictionary<string, AIFunction> s_tools;

    [Description("Searches for available flights based on the current travel plan.")]
    private static string FindFlights() => string.Empty;

    static FlightsWorkflowToolHandler()
    {
        s_tools = [];
        var function = AIFunctionFactory.Create(FindFlights, FindFlightsToolName);
        s_tools[function.Name] = function;
    }

    public string ToolName => FindFlightsToolName;

    public List<AITool> GetDeclarationOnlyTools()
    {
        return s_tools.Select(t => t.Value.AsDeclarationOnly()).Cast<AITool>().ToList();
    }

    public async IAsyncEnumerable<ToolHandlerUpdate> ExecuteAsync(
        FunctionCallContent call,
        ToolHandlerContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var request = new FlightsWorkflowRequest(context.ThreadId);

        await foreach (var evt in flightsWorkflowServiceFactory().SearchAsync(request, cancellationToken))
        {
            if (evt is FlightSearchCompleteEvent flightSearchCompleteEvent)
            {
                var flights = flightSearchCompleteEvent.Flights;
                var summary = flights.Count == 0
                    ? "No flights found for the requested route and dates."
                    : $"Found {flights.Count} flight options. " +
                      $"Prices range from {flights.Min(f => f.PricePerPerson):C} to {flights.Max(f => f.PricePerPerson):C} per person. " +
                      $"Full results are displayed in the sidebar.";

                yield return new ToolResultUpdate(
                    new FunctionResultContent(call.CallId, summary));
            }

            if (evt is FlightSearchSavedEvent savedEvent)
            {
                var artifact = new ArtifactCreated("FlightSearch", savedEvent.Id);
                yield return new ToolStateSnapshotUpdate("ArtifactCreated", artifact);
            }

            if (evt is FlightPlanValidationFailedEvent validationFailedEvent)
            {
                yield return new ToolErrorUpdate(validationFailedEvent.Message);
                yield break;
            }

            if (evt is ExecutorFailedEvent or WorkflowErrorEvent)
            {
                yield return new ToolErrorUpdate("Something went wrong while searching for flights. Please try again.");
                yield break;
            }
        }
    }
}
