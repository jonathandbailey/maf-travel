using Agents.Extensions;
using Microsoft.Agents.AI;
using System.Diagnostics;
using Travel.Experience.Application.Agents;
using Travel.Experience.Application.Agents.ToolHandling;
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

        var agent = await AgentHelper.Create("conversation.yaml", ConversationAgentTools.GetDeclarationOnlyTools());

        var workflowFactory = AgentFactoryHelper.Create();
        IConversationToolHandler[] handlers = [new TravelWorkflowToolHandler(workflowFactory)];
        var registry = new ConversationToolHandlerRegistry(handlers);

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

        var agent = await AgentHelper.CreateWithFileThread("conversation.yaml", ConversationAgentTools.GetDeclarationOnlyTools());

        var workflowFactory = AgentFactoryHelper.CreateWithFileRepositories();
        IConversationToolHandler[] handlers = [new TravelWorkflowToolHandler(workflowFactory)];
        var registry = new ConversationToolHandlerRegistry(handlers);

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