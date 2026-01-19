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

    public TravelPlanSummary(TravelPlan plan)
    {
        Origin = !string.IsNullOrEmpty(plan.Origin) ? plan.Origin : TravelPlanConstants.NotSet;
        Destination = !string.IsNullOrEmpty(plan.Destination) ? plan.Destination : TravelPlanConstants.NotSet;
        StartDate = plan.StartDate?.ToString("yyyy-MM-dd") ?? TravelPlanConstants.NotSet;
        EndDate = plan.EndDate?.ToString("yyyy-MM-dd") ?? TravelPlanConstants.NotSet;
        FlightOptionStatus = plan.FlightPlan.FlightOptionsStatus.ToString();
        UserFlightOptionStatus = plan.FlightPlan.UserFlightOptionStatus.ToString();
        TravelPlanStatus = plan.TravelPlanStatus.ToString();
    }

    public string Origin { get; set; }

    public string Destination { get; set; }

    public string StartDate { get; set; }

    public string EndDate { get; set; }

    public string FlightOptionStatus { get; set; }

    public string UserFlightOptionStatus { get; set; }

    public string TravelPlanStatus { get; set; }
}

