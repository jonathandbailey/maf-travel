
using System.Diagnostics;
using FluentAssertions;
using Travel.Tests.Common;
using Travel.Tests.Helper;
using Travel.Workflows.Events;

namespace Travel.Tests.Integration;

public class PlanningWorkflowTests : IDisposable
{
    private static readonly ActivitySource TestActivitySource = new("Travel.Tests", "1.0.0");

    public static IEnumerable<object[]> TravelPlanningScenarios()
    {
        var scenarios = ScenarioLoader.LoadPlanningWorkflowScenarios();
        foreach (var scenario in scenarios)
        {
            yield return [scenario];
        }
    }

    public PlanningWorkflowTests()
    {
        TelemetryHelper.Initialize(SettingsHelper.GetAspireDashboardSettings());
    }


    [Theory]
    [MemberData(nameof(TravelPlanningScenarios))]
    public async Task PlanningWorkflow_ShouldUpdatePlanAndRequestionInformation_WhenIncompletePlanProvided(PlanningWorkflowScenario scenario)
    {
        using var testActivity = TestActivitySource.StartActivity($"Test: {scenario.ScenarioName ?? "PlanningWorkflow"}");

        var harness = new WorkflowMockTestHarness2();

        var runIndex = 0;
        foreach (var run in scenario.Runs)
        {
            using var runActivity = TestActivitySource.StartActivity($"Run {++runIndex}");
            await harness.WatchStreamAsync(run.Message, run.AgentMetas);
        }

        harness.Events.ShouldHaveEvent().ShouldHaveType<TravelPlanningCompleteEvent>();

        var planningCompleteEvent = harness.Events.OfType<TravelPlanningCompleteEvent>().First();

        planningCompleteEvent.
            Should().NotBeNull("Expected a TravelPlanningCompleteEvent to be emitted.");

        planningCompleteEvent.
            TravelPlan.Should().
            BeEquivalentTo(scenario.ExpectedTravelPlan, "The emitted travel plan should match the expected travel plan.");

    }

    public void Dispose()
    {
        TelemetryHelper.Dispose();
    }

}

