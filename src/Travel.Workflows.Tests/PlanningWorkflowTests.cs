using Agents.Services;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Workflows.Planning.Dto;

namespace Travel.Workflows.Tests;

public class PlanningWorkflowTests
{
    private readonly Mock<IAgentFactory> _mockAgentFactory = new();

    [Fact]
    public async Task WorkflowFactory_ShouldRunWorkflow_WithToolCallResponse()
    {
        var agentResponse = TestHelper.CreateToolCallInformationRequestResponse();
        TestHelper.SetupFakeAgent(agentResponse, _mockAgentFactory);

        var workflow = await TestHelper.CreateWorkflowAsync(_mockAgentFactory);
        Assert.NotNull(workflow);

        var inputMessage = new ChatMessage(ChatRole.User, "Update my travel plan to Tokyo");

        var run = await InProcessExecution.StreamAsync(workflow, inputMessage, (string?)null);

        Assert.NotNull(run);
       
        await foreach (var evt in run.WatchStreamAsync())
        {
            if (evt is RequestInfoEvent requestInfoEvent)
            {
                var data = requestInfoEvent.Data as ExternalRequest;

                Assert.NotNull(data);

                var request = data.Data.AsType(typeof(InformationRequest)) as InformationRequest;

                Assert.NotNull(request);

                break;
            }
        }
    }
}

