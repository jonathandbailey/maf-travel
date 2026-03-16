using FluentAssertions;
using System.Diagnostics;
using Travel.Tests.Shared;
using Travel.Tests.Shared.Helper;
using Travel.Workflows.TravelPlanCriteria.Events;

namespace Travel.Tests.Integration;

public class TravelPlanningRunnerTests : IClassFixture<TelemetryFixture>
{
    private static readonly ActivitySource TestActivitySource = new("Travel.Tests", "1.0.0");

    [Theory]
    [MemberData(nameof(TravelPlanningScenarios))]
    [Trait("Category", "Integration")]
    public async Task Test_PlanningWorkflow(TravelPlanningScenario scenario)
    {
        using var testActivity = TestActivitySource.StartActivity();

        var workflowFactory = AgentFactoryHelper.Create();

        var harness = new TravelWorkflowTestHarness(workflowFactory);
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

    public static IEnumerable<object[]> TravelPlanningScenarios()
    {
        var scenarios = ScenarioLoader.LoadTravelPlanningScenarios();
        foreach (var scenario in scenarios)
        {
            yield return [scenario];
        }
    }
}