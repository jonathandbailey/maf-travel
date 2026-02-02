namespace Travel.Agents.A2A.Flights.Dto;

public enum FlightAgentStatus
{
    Success,
    Failed
}

public class FlightAgentResponseDto
{
    public Guid? FlightSearchId { get; set; }

    public string Summary { get; set; }

    public FlightAgentStatus Status { get; set; }
}

