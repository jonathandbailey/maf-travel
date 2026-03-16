using Agents.Tools;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Travel.Workflows.Flights.Dto;
using Travel.Workflows.Flights.Events;
using Travel.Workflows.Flights.Services;

namespace Travel.Experience.Application.Agents;

public sealed class FlightsWorkflowToolHandler(Func<FlightsWorkflowService> flightsWorkflowServiceFactory) : IToolHandler
{
    public const string FindFlightsToolName = "find_flights";

    private static readonly Dictionary<string, AIFunction> s_tools;
    private static readonly JsonSerializerOptions s_jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Description("Searches for available flights when a user wants to find or book a flight.")]
    private static string FindFlights(
        [Description("The flight search parameters")] FindFlightsRequest request)
        => string.Empty;

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
        var json = JsonSerializer.Serialize(call.Arguments);
        var dto = JsonSerializer.Deserialize<FindFlightsRequest>(json, s_jsonOptions);
        if (dto is null) yield break;

        if (!DateOnly.TryParse(dto.DepartureDate, out var departureDate))
        {
            yield return new ToolErrorUpdate("Invalid departure date format. Please use YYYY-MM-DD.");
            yield break;
        }

        DateOnly? returnDate = DateOnly.TryParse(dto.ReturnDate, out var parsed) ? parsed : null;

        var request = new FlightsWorkflowRequest(
            dto.Origin,
            dto.Destination,
            departureDate,
            returnDate,
            dto.Passengers);

        await foreach (var evt in flightsWorkflowServiceFactory().SearchAsync(request, cancellationToken))
        {
            if (evt is FlightSearchCompleteEvent flightSearchCompleteEvent)
            {
                yield return new ToolResultUpdate(
                    new FunctionResultContent(call.CallId, flightSearchCompleteEvent.Flights));
            }

            if (evt is ExecutorFailedEvent or WorkflowErrorEvent)
            {
                yield return new ToolErrorUpdate("Something went wrong while searching for flights. Please try again.");
                yield break;
            }
        }
    }
}
