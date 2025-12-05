namespace Application.Workflows.ReAct.Dto;

public class ActObservation(string message)
{
    public string Message { get; init; } = message;

    public string Observation { get; set; } = string.Empty;

    public string ResultType { get; set; } = string.Empty;
 
   
}

