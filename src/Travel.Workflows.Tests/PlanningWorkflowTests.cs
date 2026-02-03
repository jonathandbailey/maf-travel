using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Workflows.Planning.Services;
using System.Text.Json;

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

    [Fact]
    public async Task FakeAgent_ShouldReturnToolCall_ForUpdateTravelPlan()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        
        var updateDto = new
        {
            Origin = "New York",
            Destination = "Paris",
            StartDate = DateTimeOffset.UtcNow.AddDays(30),
            EndDate = DateTimeOffset.UtcNow.AddDays(37)
        };

        var toolCallArguments = new Dictionary<string, object?>
        {
            ["userId"] = userId,
            ["sessionId"] = sessionId,
            ["travelPlanUpdateDto"] = updateDto
        };

        var toolCallContent = new FunctionCallContent(
            callId: "call_123",
            name: "UpdateTravelPlan",
            arguments: toolCallArguments);

        var responseMessage = new ChatMessage(ChatRole.Assistant, [toolCallContent]);
        var agentResponse = new AgentResponse([responseMessage]);

        var fakeAgent = new FakeAgent(agentResponse);

        var inputMessages = new[] { new ChatMessage(ChatRole.User, "Update my travel plan") };
        var result = await fakeAgent.RunAsync(inputMessages);

        Assert.NotNull(result);
        Assert.True(fakeAgent.WasInvoked);
        Assert.Single(result.Messages);
        
        var message = result.Messages.First();
        Assert.Equal(ChatRole.Assistant, message.Role);
        
        var toolCall = message.Contents.OfType<FunctionCallContent>().FirstOrDefault();
        Assert.NotNull(toolCall);
        Assert.Equal("update_travel_plan", toolCall.Name);
        Assert.Equal("call_123", toolCall.CallId);
        
        Assert.True(toolCall.Arguments is IDictionary<string, object?>);
        var args = (IDictionary<string, object?>)toolCall.Arguments!;
        Assert.Equal(userId, args["userId"]);
        Assert.Equal(sessionId, args["sessionId"]);
    }

    [Fact]
    public async Task WorkflowFactory_ShouldRunWorkflow_WithToolCallResponse()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        
        var updateDto = new
        {
            Origin = "Seattle",
            Destination = "Tokyo",
            StartDate = DateTimeOffset.UtcNow.AddDays(60),
            EndDate = DateTimeOffset.UtcNow.AddDays(67)
        };

        var toolCallArguments = new Dictionary<string, object?>
        {
            ["userId"] = userId,
            ["sessionId"] = sessionId,
            ["travelPlanUpdateDto"] = updateDto
        };

        var toolCallContent = new FunctionCallContent(
            callId: "call_456",
            name: "update_travel_plan",
            arguments: toolCallArguments);

        var responseMessage = new ChatMessage(ChatRole.Assistant, [toolCallContent]);
        var agentResponse = new AgentResponse([responseMessage]);

        var fakeAgent = new FakeAgent(agentResponse);

        var mockAgentFactory = new Mock<IAgentFactory>();
        mockAgentFactory
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<ChatResponseFormat>(), It.IsAny<List<AITool>>()))
            .ReturnsAsync(fakeAgent);

        var workflowFactory = new WorkflowFactory(mockAgentFactory.Object);
        var workflow = await workflowFactory.Build();

        Assert.NotNull(workflow);

        var inputMessage = new ChatMessage(ChatRole.User, "Update my travel plan to Tokyo");

        var run = await InProcessExecution.StreamAsync(workflow, inputMessage, (string?)null);

        Assert.NotNull(run);

        var results = new List<object>();
        await foreach (var evt in run.WatchStreamAsync())
        {
            results.Add(evt);
        }

        Assert.NotEmpty(results);
        Assert.True(fakeAgent.WasInvoked);

        var messages = results.OfType<List<ChatMessage>>().FirstOrDefault();
        Assert.NotNull(messages);
        Assert.Single(messages);

        var message = messages.First();
        Assert.Equal(ChatRole.Assistant, message.Role);

        var toolCall = message.Contents.OfType<FunctionCallContent>().FirstOrDefault();
        Assert.NotNull(toolCall);
        Assert.Equal("UpdateTravelPlan", toolCall.Name);
        Assert.Equal("call_456", toolCall.CallId);

        var args = (IDictionary<string, object?>)toolCall.Arguments!;
        Assert.Equal(userId, args["userId"]);
        Assert.Equal(sessionId, args["sessionId"]);
    }

    private class FakeAgent : AIAgent
    {
        private readonly AgentResponse? _response;

        public bool WasInvoked { get; private set; }

        public FakeAgent()
        {
        }

        public FakeAgent(AgentResponse response)
        {
            _response = response;
        }

        protected override async Task<AgentResponse> RunCoreAsync(
            IEnumerable<ChatMessage> messages,
            AgentSession? session = null,
            AgentRunOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            WasInvoked = true;

            if (_response is not null)
            {
                return _response;
            }

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

