using Travel.Workflows.Dto;

namespace Travel.Workflows.Models;

public class TravelPlanSummary
{
    public TravelPlanSummary(TravelPlanDto planDto)
    {
        Origin = !string.IsNullOrEmpty(planDto.Origin) ? planDto.Origin : TravelPlanConstants.NotSet;
        Destination = !string.IsNullOrEmpty(planDto.Destination) ? planDto.Destination : TravelPlanConstants.NotSet;
        StartDate = planDto.StartDate?.ToString("yyyy-MM-dd") ?? TravelPlanConstants.NotSet;
        EndDate = planDto.EndDate?.ToString("yyyy-MM-dd") ?? TravelPlanConstants.NotSet;
        FlightOptionStatus = planDto.FlightOptionsStatus.ToString();
        UserFlightOptionStatus = planDto.UserFlightOptionStatus.ToString();
        TravelPlanStatus = planDto.TravelPlanStatus.ToString();
    }

    public string Origin { get; set; }

    public string Destination { get; set; }

    public string StartDate { get; set; }

    public string EndDate { get; set; }

    public string FlightOptionStatus { get; set; }

    public string UserFlightOptionStatus { get; set; }

    public string TravelPlanStatus { get; set; }
}

public static class TravelPlanConstants
{
    public const string NotSet = "Not_Set";
}

