using System.ComponentModel;

namespace Travel.Experience.Application.Agents;

public record FindFlightsRequest(
    [property: Description("Departure airport code or city")] string Origin,
    [property: Description("Arrival airport code or city")] string Destination,
    [property: Description("Departure date in YYYY-MM-DD format")] string DepartureDate,
    [property: Description("Optional return date in YYYY-MM-DD format")] string? ReturnDate,
    [property: Description("Number of passengers")] int Passengers = 1);
