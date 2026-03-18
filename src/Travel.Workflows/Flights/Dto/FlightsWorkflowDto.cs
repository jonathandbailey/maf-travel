namespace Travel.Workflows.Flights.Dto;

// Passed to InProcessExecution (derived from TravelPlanState)
public record FlightsWorkflowInput(
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    DateOnly? ReturnDate,
    int Passengers);

// StartNode → FlightNode
public record FlightSearchAgentCommand(
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    DateOnly? ReturnDate,
    int Passengers);

// FlightNode → SearchNode (parsed from agent tool call)
public record FlightSearchCommand(
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    DateOnly? ReturnDate,
    int Passengers);

// SearchNode → FlightsEndNode
public record FlightSearchResult(IReadOnlyList<FlightOption> Flights);

// Local DTO mirroring the MCP server's FlightOption JSON shape
public record FlightOption(
    string FlightNumber,
    string Airline,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    decimal PricePerPerson,
    int AvailableSeats);
