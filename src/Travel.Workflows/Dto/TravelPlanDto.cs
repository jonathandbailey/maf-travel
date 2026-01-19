using System.Text.Json.Serialization;
using Travel.Workflows.Models;
using Travel.Workflows.Models.Flights;

namespace Travel.Workflows.Dto;

public class TravelPlanDto
{
    [JsonConstructor]
    public TravelPlanDto(string? origin, string? destination, DateTimeOffset? startDate, DateTimeOffset? endDate, FlightOptionsStatus flightOptionsStatus, UserFlightOptionsStatus userFlightOptionStatus, TravelPlanStatus travelPlanStatus)
    {
        Origin = origin;
        Destination = destination;
        StartDate = startDate;
        EndDate = endDate;
        FlightOptionsStatus = flightOptionsStatus;
        UserFlightOptionStatus = userFlightOptionStatus;
        TravelPlanStatus = travelPlanStatus;
    }

    public string? Origin { get; }

    public string? Destination { get; }

    public DateTimeOffset? StartDate { get; }

    public DateTimeOffset? EndDate { get; }

    public FlightOptionsStatus FlightOptionsStatus { get; private set; }


    public UserFlightOptionsStatus UserFlightOptionStatus { get; private set; }

    public TravelPlanStatus TravelPlanStatus { get; private set; }
}