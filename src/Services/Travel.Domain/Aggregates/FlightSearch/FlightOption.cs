namespace Travel.Domain.Aggregates.FlightSearch;

public class FlightOption : BaseEntity
{
    public string FlightNumber { get; private set; } = default!;
    public string Airline { get; private set; } = default!;
    public string Origin { get; private set; } = default!;
    public string Destination { get; private set; } = default!;
    public DateTime DepartureTime { get; private set; }
    public DateTime ArrivalTime { get; private set; }
    public decimal PricePerPerson { get; private set; }
    public int AvailableSeats { get; private set; }

    private FlightOption() { }

    public static FlightOption Create(
        string flightNumber,
        string airline,
        string origin,
        string destination,
        DateTime departureTime,
        DateTime arrivalTime,
        decimal pricePerPerson,
        int availableSeats) =>
        new()
        {
            FlightNumber = flightNumber,
            Airline = airline,
            Origin = origin,
            Destination = destination,
            DepartureTime = departureTime,
            ArrivalTime = arrivalTime,
            PricePerPerson = pricePerPerson,
            AvailableSeats = availableSeats
        };

    public static FlightOption Reconstitute(
        Guid id,
        string flightNumber,
        string airline,
        string origin,
        string destination,
        DateTime departureTime,
        DateTime arrivalTime,
        decimal pricePerPerson,
        int availableSeats) =>
        new()
        {
            Id = id,
            FlightNumber = flightNumber,
            Airline = airline,
            Origin = origin,
            Destination = destination,
            DepartureTime = departureTime,
            ArrivalTime = arrivalTime,
            PricePerPerson = pricePerPerson,
            AvailableSeats = availableSeats
        };
}
