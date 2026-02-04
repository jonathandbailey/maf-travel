using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Workflows.Planning.Services;

namespace Travel.Workflows.Tests;

public class PlanningWorkflowTests
{
    private readonly Mock<IAgentFactory> _mockAgentFactory = new();

    [Fact]
    public async Task WorkflowFactory_ShouldRunWorkflow_WithToolCallResponse()
    {
        var agentResponse = CreateToolCallResponse();
        SetupFakeAgent(agentResponse);

        var workflow = await CreateWorkflowAsync();
        Assert.NotNull(workflow);

        var inputMessage = new ChatMessage(ChatRole.User, "Update my travel plan to Tokyo");

        var run = await InProcessExecution.StreamAsync(workflow, inputMessage, (string?)null);

        Assert.NotNull(run);

        var results = new List<object>();
       
        await foreach (var evt in run.WatchStreamAsync())
        {
            results.Add(evt);
        }
    }

    private FakeAgent SetupFakeAgent(AgentResponse agentResponse)
    {
        var fakeAgent = new FakeAgent([agentResponse]);
        _mockAgentFactory
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<ChatResponseFormat>(), It.IsAny<List<AITool>>()))
            .ReturnsAsync(fakeAgent);
        return fakeAgent;
    }

    private async Task<Workflow> CreateWorkflowAsync()
    {
        var workflowFactory = new WorkflowFactory(_mockAgentFactory.Object);
        return await workflowFactory.Build();
    }

    private static AgentResponse CreateToolCallResponse()
    {
        var updateDto = new
        {
            Origin = "Seattle",
            Destination = "Tokyo",
            StartDate = DateTimeOffset.UtcNow.AddDays(60),
            EndDate = DateTimeOffset.UtcNow.AddDays(67)
        };

        var arguments = new Dictionary<string, object?>
        {
            ["travelPlanUpdateDto"] = updateDto
        };

        var toolCallContent = new FunctionCallContent(
            callId: "call_456",
            name: "update_travel_plan",
            arguments: arguments);

        var responseMessage = new ChatMessage(ChatRole.Assistant, [toolCallContent]);
        return new AgentResponse([responseMessage]);
    }
}

