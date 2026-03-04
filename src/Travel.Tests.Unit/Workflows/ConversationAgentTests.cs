using FluentAssertions;
using System.Diagnostics;
using Travel.Tests.Shared;
using Travel.Tests.Unit.Common;
using Travel.Tests.Unit.TestData;

namespace Travel.Tests.Unit.Workflows;

public class ConversationAgentTests : IClassFixture<TelemetryFixture>
{
    private static readonly ActivitySource TestActivitySource = new("Travel.Tests", "1.0.0");

    [Theory]
  
    [MemberData(nameof(Scenarios))]
    public async Task ConversationAgent_ShouldForwardToolHandlerUpdates_WhenAgentInvokesTool(ConversationAgentScenario scenario)
    {
        using var testActivity = TestActivitySource.StartActivity($"TestCase: {scenario.ScenarioName}");

        var threadId = Guid.NewGuid();
        var harness = new ConversationAgentMockTestHarness();

        var runIndex = 0;
        foreach (var run in scenario.Runs)
        {
            using var runActivity = TestActivitySource.StartActivity($"Run - {++runIndex}");
            await harness.RunAsync(run, threadId);
        }

        harness.Updates.Should().NotBeEmpty("ConversationAgent should forward at least one update from the tool handler");

        var travelPlanSnapshots = harness.GetTravelPlanSnapshots().ToList();
        travelPlanSnapshots.Should().NotBeEmpty("at least one TravelPlanUpdate state snapshot should have been forwarded");

        travelPlanSnapshots.Last().Should().BeEquivalentTo(
            scenario.ExpectedTravelPlan,
            "the final travel plan snapshot should match the expected travel plan");
    }

    public static IEnumerable<object[]> Scenarios()
    {
        var scenarios = UnitTestsScenarioLoader.LoadConversationAgentScenarios();
        foreach (var scenario in scenarios)
        {
            yield return [scenario];
        }
    }
}
