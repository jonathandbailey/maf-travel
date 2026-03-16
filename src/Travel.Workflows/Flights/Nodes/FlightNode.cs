using System.Diagnostics;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Services;
using Travel.Workflows.Common;
using Travel.Workflows.Common.Exceptions;
using Travel.Workflows.Common.Extensions;
using Travel.Workflows.Common.Telemetry;
using Travel.Workflows.Flights.Dto;

namespace Travel.Workflows.Flights.Nodes;

public partial class FlightNode(AIAgent agent) : Executor(FlightsNodeNames.FlightNode)
{
    [MessageHandler(Send = [typeof(FlightSearchCommand)])]
    private async ValueTask HandleAsync(FlightSearchAgentCommand command, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = TravelWorkflowTelemetry.InvokeNode(FlightsNodeNames.FlightNode, Guid.Empty);

        var input = JsonSerializer.Serialize(command, Json.FunctionCallSerializerOptions);
        activity?.AddNodeAgentInput(input);

        AgentResponse response;
        try
        {
            response = await agent.RunAsync(input, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw new WorkflowException("FlightNode failed to get a response from the flight agent.", FlightsNodeNames.FlightNode, Guid.Empty, ex);
        }

        activity?.AddNodeAgentOutput(response.Text);
        response.TraceToolCalls(activity);

        var toolCall = response.ExtractToolCalls()
            .FirstOrDefault(c => c.Name == FlightSearchToolsHandler.SearchFlightsToolName);

        if (toolCall == null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Agent did not return search_flights tool call.");
            throw new WorkflowException("FlightNode: agent did not call search_flights.", FlightsNodeNames.FlightNode, Guid.Empty);
        }

        toolCall.TryGetArgument<string>("origin", out var origin, Json.FunctionCallSerializerOptions);
        toolCall.TryGetArgument<string>("destination", out var destination, Json.FunctionCallSerializerOptions);
        toolCall.TryGetArgument<string>("departureDate", out var departureDateRaw, Json.FunctionCallSerializerOptions);
        toolCall.TryGetArgument<string>("returnDate", out var returnDateRaw, Json.FunctionCallSerializerOptions);
        toolCall.TryGetArgument<int>("passengers", out var passengers, Json.FunctionCallSerializerOptions);

        var departureDate = departureDateRaw != null
            ? DateOnly.Parse(departureDateRaw)
            : command.DepartureDate;

        var returnDate = returnDateRaw != null
            ? DateOnly.Parse(returnDateRaw)
            : command.ReturnDate;

        await context.SendMessageAsync(
            new FlightSearchCommand(
                origin ?? command.Origin,
                destination ?? command.Destination,
                departureDate,
                returnDate,
                passengers > 0 ? passengers : command.Passengers),
            cancellationToken);
    }
}
