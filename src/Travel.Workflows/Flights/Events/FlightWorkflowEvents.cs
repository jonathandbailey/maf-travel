using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Flights.Dto;

namespace Travel.Workflows.Flights.Events;

public class FlightSearchCompleteEvent(IReadOnlyList<FlightOption> flights) : WorkflowEvent
{
    public IReadOnlyList<FlightOption> Flights { get; } = flights;
}

public class FlightSearchSavedEvent(Guid id) : WorkflowEvent
{
    public Guid Id { get; } = id;
}
