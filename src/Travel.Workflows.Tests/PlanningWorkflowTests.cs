using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Workflows.Planning.Services;

namespace Travel.Workflows.Tests;

public class PlanningWorkflowTests
{
    [Fact]
    public async Task WorkflowFactory_ShouldCreateWorkflow_WithMockedAgentFactory()
    {
        var mockAgentFactory = new Mock<IAgentFactory>();
        var mockAgent = new Mock<AIAgent>();

        mockAgentFactory
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<ChatResponseFormat>(), It.IsAny<List<AITool>>()))
            .ReturnsAsync(mockAgent.Object);

        var workflowFactory = new WorkflowFactory(mockAgentFactory.Object);

        var workflow = await workflowFactory.Build();

        Assert.NotNull(workflow);
        mockAgentFactory.Verify(x => x.Create("planning_agent", null, null), Times.Once);
    }

    [Fact]
    public async Task WorkflowFactory_ShouldCreateWorkflow_WithFakeAgent()
    {
        var mockAgentFactory = new Mock<IAgentFactory>();
        var fakeAgent = new FakeAgent();

        mockAgentFactory
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<ChatResponseFormat>(), It.IsAny<List<AITool>>()))
            .ReturnsAsync(fakeAgent);

        var workflowFactory = new WorkflowFactory(mockAgentFactory.Object);

        var workflow = await workflowFactory.Build();

        Assert.NotNull(workflow);

        var startMessage = new ChatMessage(ChatRole.User, "Plan a trip to Paris");
        var agentResponse = await fakeAgent.RunAsync([startMessage]);

        Assert.NotNull(agentResponse);
        Assert.True(fakeAgent.WasInvoked);
        Assert.Single(agentResponse.Messages);
    }

    [Fact]
    public async Task WorkflowFactory_ShouldRunWorkflow_WithFakeAgent()
    {
        var mockAgentFactory = new Mock<IAgentFactory>();
        var fakeAgent = new FakeAgent();

        mockAgentFactory
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<ChatResponseFormat>(), It.IsAny<List<AITool>>()))
            .ReturnsAsync(fakeAgent);

        var workflowFactory = new WorkflowFactory(mockAgentFactory.Object);

        var workflow = await workflowFactory.Build();

        Assert.NotNull(workflow);

        var inputMessage = new ChatMessage(ChatRole.User, "Plan a vacation to Paris");

        var run = await InProcessExecution.StreamAsync(workflow, inputMessage, (string?)null);

        Assert.NotNull(run);

        var results = new List<object>();
        await foreach (var evt in run.WatchStreamAsync())
        {
            results.Add(evt);
        }

        Assert.NotEmpty(results);
        Assert.True(fakeAgent.WasInvoked);
    }

    private class FakeAgent : AIAgent
    {
        public bool WasInvoked { get; private set; }

        protected override async Task<AgentResponse> RunCoreAsync(
            IEnumerable<ChatMessage> messages,
            AgentSession? session = null,
            AgentRunOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            WasInvoked = true;

            var responseMessages = new List<ChatMessage>
            {
                new(ChatRole.Assistant, "This is a fake planning response.")
            };

            return new AgentResponse([.. responseMessages]);
        }

        protected override IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
            IEnumerable<ChatMessage> messages,
            AgentSession? session = null,
            AgentRunOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<AgentSession> GetNewSessionAsync(CancellationToken cancellationToken = default)
        {
            var mockSession = new Mock<AgentSession>();
            return new ValueTask<AgentSession>(mockSession.Object);
        }

        public override ValueTask<AgentSession> DeserializeSessionAsync(
            System.Text.Json.JsonElement json,
            System.Text.Json.JsonSerializerOptions? jsonOptions = null,
            CancellationToken cancellationToken = default)
        {
            var mockSession = new Mock<AgentSession>();
            return new ValueTask<AgentSession>(mockSession.Object);
        }
    }
}

