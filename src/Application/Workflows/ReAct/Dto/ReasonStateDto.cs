namespace Application.Workflows.ReAct.Dto;

using System.Collections.Generic;
using System.Numerics;

public class ReasonState
{
    /// <summary>
    /// List of chosen capability names, e.g. ["research_flights", "research_hotels"].
    /// Null means capability has not been selected yet.
    /// </summary>
    public List<string>? Capabilities { get; set; }

    /// <summary>
    /// Known slot values collected so far. 
    /// Keys must always include: origin, destination, depart_date, return_date.
    /// Values may be null until provided by the user.
    /// </summary>
    public Dictionary<string, string?> KnownInputs { get; set; } = new();

    /// <summary>
    /// Keys for required inputs that are missing for the chosen capabilities.
    /// This is computed by the Reason agent, not user input.
    /// </summary>
    public List<string> MissingInputs { get; set; } = new();

    /// <summary>
    /// Next action returned by the Reason agent (AskUser or Orchestrate).
    /// This is a polymorphic container for planning execution or asking for more info.
    /// </summary>
    public NextAction NextAction { get; set; } = new();
}

public class NextAction
{
    /// <summary>
    /// The type of action to perform next ("AskUser" or "Orchestrate").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Parameters associated with the next action.
    /// </summary>
    public NextActionParameters Parameters { get; set; } = new();
}

public class NextActionParameters
{
    /// <summary>
    /// Questions to ask the user (when Type = "AskUser").
    /// </summary>
    public List<string>? Questions { get; set; }

    /// <summary>
    /// A machine-readable plan (when Type = "Orchestrate").
    /// Contains capabilities and resolved inputs for downstream worker agents.
    /// </summary>
    public Plan? Plan { get; set; }
}

public class Plan
{
    /// <summary>
    /// Capabilities to execute, e.g. ["research_flights","research_hotels"].
    /// </summary>
    public List<string> Capabilities { get; set; } = new();

    /// <summary>
    /// Fully resolved input values required to execute the plan.
    /// </summary>
    public Dictionary<string, string?> Inputs { get; set; } = new();
}



