namespace Travel.Application.Api.Dto;

public class TravelPlanUpdateDto()
{
    public string? Origin { get; set; }

    public string? Destination { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }
}

public class TravelPlanDto(string? origin, string? destination, DateTimeOffset? startDate, DateTimeOffset? endDate)
{
    public string? Origin { get;  } = origin;

    public string? Destination { get;  } = destination;

    public DateTimeOffset? StartDate { get;  } = startDate;

    public DateTimeOffset? EndDate { get;  } = endDate;
}


