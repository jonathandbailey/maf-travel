using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents.Dto;

public class ReasoningOutputDto
{
    public string Thought { get; set; } = string.Empty;

    public NextAction NextAction { get; set; }

    public string Status { get; set; } = string.Empty;

    public TravelPlanUpdateDto? TravelPlanUpdate { get; set; }

    public UserInputRequestDto? UserInput { get; set; }

    public void Error()
    {

    }
}

public enum NextAction
{
    RequestInformation,
    FlightAgent,
    Error
}

public class TravelPlanUpdateDto
{
    public string? Origin { get; set; }

    public string? Destination { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}

public class UserInputRequestDto()
{
    public string Question { get; set; } = string.Empty;

    public List<string> RequiredInputs { get; set; } = new List<string>();
}