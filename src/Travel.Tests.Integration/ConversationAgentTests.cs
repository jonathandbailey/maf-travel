using Agents.Extensions;
using Microsoft.Agents.AI;
using System.Diagnostics;
using Agents.Tools;
using Travel.Experience.Application.Agents;
using Travel.Tests.Shared;
using Travel.Tests.Shared.Helper;

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

        var threadId = Guid.NewGuid();

        var workflowFactory = AgentFactoryHelper.Create();
        var handler = new TravelWorkflowToolHandler(workflowFactory);

        var agent = await AgentHelper.Create("conversation.yaml", handler.GetDeclarationOnlyTools());

        IToolHandler[] handlers = [handler];
        var registry = new ToolRegistry(handlers);

        var conversationAgent = new ConversationAgent(agent, registry);
    
        var agentRunOptions = new ChatClientAgentRunOptions();

        agentRunOptions.AddAgUiThreadId(threadId.ToString());

        var agentSession = await conversationAgent.CreateSessionAsync();

        var runIndex = 0;

        foreach (var message in scenario.Messages)
        {
            using var runActivity = TestActivitySource.StartActivity($"Run {++runIndex}");

            var responses = await conversationAgent.RunStreamingAsync(message, agentSession, options: agentRunOptions,
                cancellationToken: CancellationToken.None).ToListAsync();
        }
    }

    [Theory]
    [Trait("Category", "Integration")]
    [MemberData(nameof(TravelPlanningScenarios))]
    public async Task TravelPlanAgent_WithFileRepositories_WhenProvidedWithCompletePlan_ShouldCompleteTheWorkflow(TravelPlanningScenario scenario)
    {
        using var testActivity = TestActivitySource.StartActivity();

        var threadId = Guid.NewGuid();

        var workflowFactory = AgentFactoryHelper.CreateWithFileRepositories();
        var handler = new TravelWorkflowToolHandler(workflowFactory);

        var agent = await AgentHelper.CreateWithFileThread("conversation.yaml", handler.GetDeclarationOnlyTools());

        IToolHandler[] handlers = [handler];
        var registry = new ToolRegistry(handlers);

        var conversationAgent = new ConversationAgent(agent, registry);

        var agentRunOptions = new ChatClientAgentRunOptions();

        agentRunOptions.AddAgUiThreadId(threadId.ToString());

        var agentSession = await conversationAgent.CreateSessionAsync();

        var runIndex = 0;

        foreach (var message in scenario.Messages)
        {
            using var runActivity = TestActivitySource.StartActivity($"Run {++runIndex}");

            var responses = await conversationAgent.RunStreamingAsync(message, agentSession, options: agentRunOptions,
                cancellationToken: CancellationToken.None).ToListAsync();
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