using Travel.Domain.Events;

namespace Travel.Domain.Aggregates.FlightSearch;

public class FlightSearch : AggregateRoot
{
    private readonly List<FlightOption> _flightOptions = new();

    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<FlightOption> FlightOptions => _flightOptions.AsReadOnly();

    private FlightSearch() { }

    public static FlightSearch Create(IReadOnlyList<FlightOption> flightOptions)
    {
        var search = new FlightSearch
        {
            CreatedAt = DateTime.UtcNow
        };
        search._flightOptions.AddRange(flightOptions);
        search.AddDomainEvent(new FlightSearchCreatedEvent(search.Id));
        return search;
    }

    public static FlightSearch Reconstitute(
        Guid id,
        DateTime createdAt,
        IReadOnlyList<FlightOption> flightOptions)
    {
        var search = new FlightSearch
        {
            Id = id,
            CreatedAt = createdAt
        };
        search._flightOptions.AddRange(flightOptions);
        return search;
    }
}
