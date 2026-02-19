using System.Diagnostics;
using FluentAssertions;
using Travel.Tests.Common;
using Travel.Tests.Helper;
using Travel.Tests.Shared;
using Travel.Workflows.Events;

namespace Travel.Tests.Integration;

public class TravelPlanningWorkflowTests : IDisposable
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

    public TravelPlanningWorkflowTests()
    {
        TelemetryHelper.Initialize(SettingsHelper.GetAspireDashboardSettings());
    }

    public void Dispose()
    {
        TelemetryHelper.Dispose();
    }

    [Theory]
    [MemberData(nameof(TravelPlanningScenarios))]
    [Trait("Category", "Integration")]
    public async Task Test_PlanningWorkflow(TravelPlanningScenario scenario)
    {
        using var testActivity = TestActivitySource.StartActivity();

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