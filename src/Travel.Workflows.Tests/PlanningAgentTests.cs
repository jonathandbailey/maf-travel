using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Workflows.Planning.Services;

namespace Travel.Workflows.Tests;

public class PlanningAgentTests
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

        var fakeAgent = new FakeAgent([agentResponse]);

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


}