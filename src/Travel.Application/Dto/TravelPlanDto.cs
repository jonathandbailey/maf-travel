using Travel.Application.Api.Dto;
using Travel.Application.Domain;
using Travel.Application.Domain.Flights;

namespace Travel.Application.Dto;

public class TravelPlanUpdateDto()
{
    public string? Origin { get; set; }

    public string? Destination { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }
  
}

public class TravelPlanDto(string? origin, string? destination, DateTimeOffset? startDate, DateTimeOffset? endDate, FlightOptionsStatus flightOptionsStatus, UserFlightOptionsStatus userFlightOptionStatus, TravelPlanStatus travelPlanStatus, Guid id, FlightPlanDto flightPlan)
{
    public Guid Id { get; } = id;

    public string? Origin { get;  } = origin;

    public string? Destination { get;  } = destination;

    public DateTimeOffset? StartDate { get;  } = startDate;

    public DateTimeOffset? EndDate { get;  } = endDate;

    public FlightOptionsStatus FlightOptionsStatus { get; private set; } = flightOptionsStatus;

  
    public UserFlightOptionsStatus UserFlightOptionStatus { get; private set; } = userFlightOptionStatus;

    public TravelPlanStatus TravelPlanStatus { get; private set; } = travelPlanStatus;

    public FlightPlanDto FlightPlan { get; } = flightPlan;
}


