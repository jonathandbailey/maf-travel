namespace Travel.Infrastructure.Documents;

internal record FlightOptionDocument(
    Guid Id,
    string FlightNumber,
    string Airline,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    decimal PricePerPerson,
    int AvailableSeats);

internal record FlightSearchDocument(
    Guid Id,
    DateTime CreatedAt,
    IReadOnlyList<FlightOptionDocument> FlightOptions);
