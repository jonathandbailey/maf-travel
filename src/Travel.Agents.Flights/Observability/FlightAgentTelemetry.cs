using System.Diagnostics;

namespace Travel.Agents.Flights.Observability;

public static class FlightAgentTelemetry
{
    private const string Name = "Flight";
    
    private static readonly ActivitySource Source = new ActivitySource("Travel.Agents", "1.0.0");

    public static Activity? Start(string input, string threadId)
    {
        var tags = new ActivityTagsCollection
        {
            { "gen_ai.agent.name", Name },
            { "gen_ai.prompt", input },
            { "gen_ai.conversation.id", threadId }
        };

        var source = Source.StartActivity($"invoke_agent {Name}", ActivityKind.Internal, null, tags);
     
        return source;
    }
}