
using System.Diagnostics;
using FluentAssertions;
using Travel.Tests.Shared;
using Travel.Tests.Shared.Helper;
using Travel.Tests.Unit.Common;
using Travel.Tests.Unit.TestData;
using Travel.Workflows.Events;
using PlanningWorkflowScenario = Travel.Tests.Unit.TestData.PlanningWorkflowScenario;

namespace Travel.Tests.Unit.Workflows;

public class TravelBookingDetailsWorkflowTests :  IClassFixture<TelemetryFixture>
{
    private static readonly ActivitySource TestActivitySource = new("Travel.Tests", "1.0.0");

    [Theory]
    [MemberData(nameof(TravelPlanningScenarios))]
    public async Task PlanningWorkflow_ShouldUpdatePlanAndRequestionInformation_WhenIncompletePlanProvided(PlanningWorkflowScenario scenario)
    {
        using var testActivity = TestActivitySource.StartActivity($"TestCase: {scenario.ScenarioName}");

        var harness = new WorkflowMockTestHarness();

        var runIndex = 0;
        foreach (var run in scenario.Runs)
        {
            using var runActivity = TestActivitySource.StartActivity($"Run - {++runIndex}");
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

    public static IEnumerable<object[]> TravelPlanningScenarios()
    {
        var scenarios = UnitTestsScenarioLoader.LoadPlanningWorkflowScenarios();
        foreach (var scenario in scenarios)
        {
            yield return [scenario];
        }
    }
}

