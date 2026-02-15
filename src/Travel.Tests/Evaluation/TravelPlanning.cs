using FluentAssertions;
using System.Diagnostics;
using Travel.Tests.Common;
using Travel.Tests.Helper;
using Travel.Workflows.Events;

namespace Travel.Tests.Evaluation;

public class TravelPlanning : IDisposable
{
    private static readonly ActivitySource TestActivitySource = new("Travel.Tests", "1.0.0");

    public static IEnumerable<object[]> TravelPlanningScenarios()
    {
        var scenarios = ScenarioLoader.LoadTravelPlanningScenarios();
        foreach (var scenario in scenarios)
        {
            yield return [scenario];
        }
    }

    public TravelPlanning()
    {
        TelemetryHelper.Initialize(SettingsHelper.GetAspireDashboardSettings());
    }

    public void Dispose()
    {
        TelemetryHelper.Dispose();
    }

    [Theory]
    [MemberData(nameof(TravelPlanningScenarios))]
    public async Task Test1(TravelPlanningScenario scenario)
    {
        using var testActivity = TestActivitySource.StartActivity("Test: PlanningWorkflow");


        var harness = new TravelWorkflowTestHarness();
        var runIndex = 0;
        foreach (var message in scenario.Messages)
        {
            using var runActivity = TestActivitySource.StartActivity($"Run {++runIndex}");
            await harness.WatchStreamAsync(message);
        }

        harness.Events.ShouldHaveEvent().ShouldHaveType<TravelPlanningCompleteEvent>();

        var planningCompleteEvent = harness.Events.OfType<TravelPlanningCompleteEvent>().First();

        planningCompleteEvent.
            Should().NotBeNull("Expected a TravelPlanningCompleteEvent to be emitted.");

        planningCompleteEvent.
            TravelPlan.Should().
            BeEquivalentTo(scenario.ExpectedTravelPlan, "The emitted travel plan should match the expected travel plan.");
    }
}