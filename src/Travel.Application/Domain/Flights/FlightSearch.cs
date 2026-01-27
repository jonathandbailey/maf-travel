namespace Travel.Application.Domain.Flights;

public class FlightSearch
{
    public FlightSearch(List<FlightOption> departureFlightOptions, List<FlightOption> returnFlightOptions)
    {
        DepartureFlightOptions = departureFlightOptions;
        ReturnFlightOptions = returnFlightOptions;
    }

    public List<FlightOption> DepartureFlightOptions { get; set; }
    public List<FlightOption> ReturnFlightOptions { get; set; }

    public Guid Id { get; set; } = Guid.NewGuid();
}
