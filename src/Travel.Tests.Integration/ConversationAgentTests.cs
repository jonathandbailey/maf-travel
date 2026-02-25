using Agents.Extensions;
using Microsoft.Agents.AI;
using Moq;
using System.Diagnostics;
using Travel.Agents.Services;
using Travel.Experience.Application.Agents;
using Travel.Tests.Shared;
using Travel.Tests.Shared.Helper;
using Travel.Workflows.Interfaces;
using Travel.Workflows.Services;

namespace Travel.Tests.Integration;

public class ConversationAgentTests : IClassFixture<TelemetryFixture>
{
    private static readonly ActivitySource TestActivitySource = new("Travel.Tests", "1.0.0");

    [Theory]
    [Trait("Category", "Integration")]
    [MemberData(nameof(TravelPlanningScenarios))]
    public async Task TravelPlanAgent_WhenProvidedWithCompletePlan_ShouldCompleteTheWorkflow(TravelPlanningScenario scenario)
    {
        using var testActivity = TestActivitySource.StartActivity();

        var threadId = Guid.NewGuid().ToString();

        var agent = await AgentHelper.Create("conversation.yaml", ConversationAgentTools.GetDeclarationOnlyTools());

        var mockFactory = new Mock<IWorkflowFactory>();

        var repo = new InMemoryCheckpointRepository();
        var sessionRepo = new InMemoryWorkflowSessionRepository();

        var agentProvider = new AgentProvider(
            AgentHelper.CreateAgentFactory(),
            AgentHelper.CreateAgentTemplateRepository());

        var service = new TravelWorkflowService(repo, sessionRepo, agentProvider);

        mockFactory.Setup(x => x.Create()).ReturnsAsync(service);

        var conversationAgent = new ConversationAgent(agent, mockFactory.Object);
    
        var agentRunOptions = new ChatClientAgentRunOptions();

        agentRunOptions.AddAgUiThreadId(threadId);

        var agentSession = await conversationAgent.CreateSessionAsync();

        var runIndex = 0;

        foreach (var scenarioMessage in scenario.Messages)
        {
            using var runActivity = TestActivitySource.StartActivity($"Run {++runIndex}");

            await foreach (var response in conversationAgent.RunStreamingAsync(scenarioMessage, agentSession, options: agentRunOptions,
                               cancellationToken: CancellationToken.None))
            {

            }
        }
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