using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Flights.Dto;

namespace Travel.Workflows.Flights.Events;

public class FlightSearchCompleteEvent(IReadOnlyList<FlightOption> flights) : WorkflowEvent
{
    public IReadOnlyList<FlightOption> Flights { get; } = flights;
}
