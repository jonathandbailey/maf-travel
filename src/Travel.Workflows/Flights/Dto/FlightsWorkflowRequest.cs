namespace Travel.Workflows.Flights.Dto;

public record FlightsWorkflowRequest(
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    DateOnly? ReturnDate,
    int Passengers);