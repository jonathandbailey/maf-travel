using System.Diagnostics;

namespace Travel.Workflows.Observability;

public static class Telemetry
{
    private static readonly ActivitySource Source = new ActivitySource("Application.Workflows", "1.0.0");

    public static Activity? Start(string name)
    {
        return Source.StartActivity(name);
    }
}