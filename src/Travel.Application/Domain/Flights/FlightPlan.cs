namespace Travel.Application.Domain.Flights;

public class FlightPlan
{
    public FlightOptionsStatus FlightOptionsStatus { get; private set; } = FlightOptionsStatus.NotCreated;

    public UserFlightOptionsStatus UserFlightOptionStatus { get; private set; } = UserFlightOptionsStatus.NotSelected;

    public List<FlightOptionSearch> FlightOptions { get; private set; } = [];

    public FlightOption? FlightOption { get; private set; }

    public FlightPlan() {}

    public FlightPlan(
        FlightOptionsStatus flightOptionsStatus, 
        UserFlightOptionsStatus userFlightOptionsStatus, 
        FlightOption flightOption,
        List<FlightOptionSearch> flightOptions
        )
    {
        FlightOptionsStatus = flightOptionsStatus;
        UserFlightOptionStatus = userFlightOptionsStatus;
        FlightOption = flightOption;
        FlightOptions = flightOptions;
    }

    public void SelectFlightOption(FlightOption flightOption)
    {
        FlightOption = flightOption;
        UserFlightOptionStatus = UserFlightOptionsStatus.Selected;
    }

    public void AddFlightOptions(FlightOptionSearch flightOptions)
    {
        FlightOptions.Add(flightOptions);

        FlightOptionsStatus = FlightOptionsStatus.Created;
        UserFlightOptionStatus = UserFlightOptionsStatus.UserChoiceRequired;
    }
}