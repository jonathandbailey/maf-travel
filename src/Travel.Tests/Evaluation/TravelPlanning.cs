using FluentAssertions;
using Travel.Tests.Common;
using Travel.Tests.Helper;
using Travel.Workflows.Events;

namespace Travel.Tests.Evaluation;

public class TravelPlanning
{
    public static IEnumerable<object[]> TravelPlanningScenarios()
    {
        var scenarios = ScenarioLoader.LoadTravelPlanningScenarios();
        foreach (var scenario in scenarios)
        {
            yield return [scenario];
        }
    }

    [Theory]
    [MemberData(nameof(TravelPlanningScenarios))]
    public async Task Test1(TravelPlanningScenario scenario)
    {
        var harness = new TravelWorkflowTestHarness();

        foreach (var message in scenario.Messages)
        {
            await harness.WatchStreamAsync(message);
        }

        var planningCompleteEvent = harness.Events.OfType<TravelPlanningCompleteEvent>().FirstOrDefault();

        planningCompleteEvent.
            Should().NotBeNull("Expected a TravelPlanningCompleteEvent to be emitted.");

        planningCompleteEvent.
            TravelPlan.Should().
            BeEquivalentTo(scenario.ExpectedTravelPlan, "The emitted travel plan should match the expected travel plan.");
    }
}