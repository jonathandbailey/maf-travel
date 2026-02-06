using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Workflows.Planning.Dto;
using Travel.Workflows.Planning.Services;

namespace Travel.Workflows.Tests;

public class TestHelper
{
    public static FakeAgent SetupFakeAgent(List<AgentResponse> agentResponse, Mock<IAgentFactory> mockAgentFactory)
    {
        var fakeAgent = new FakeAgent(agentResponse);
        mockAgentFactory
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<ChatResponseFormat>(), It.IsAny<List<AITool>>()))
            .ReturnsAsync(fakeAgent);
        return fakeAgent;
    }

    public static async Task<Workflow> CreateWorkflowAsync(Mock<IAgentFactory> mockAgentFactory)
    {
        var workflowFactory = new WorkflowFactory(mockAgentFactory.Object);
        return await workflowFactory.Build();
    }

    public static AgentResponse CreateToolCallTravelPlanUpdateResponse()
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

    public static AgentResponse CreateToolCallTravelPlanFinalizeResponse()
    {
        var toolCallContent = new FunctionCallContent(
            callId: "call_451",
            name: "finalize_travel_plan");

        var responseMessage = new ChatMessage(ChatRole.Assistant, [toolCallContent]);
        return new AgentResponse([responseMessage]);
    }

    public static AgentResponse CreateToolCallInformationRequestResponse()
    {
        var informationRequest =
            Create();

        var arguments = new Dictionary<string, object?>
        {
            ["informationRequest"] = informationRequest
        };

        var toolCallContent = new FunctionCallContent(
            callId: "call_123",
            name: "information_request",
            arguments: arguments);

        var responseMessage = new ChatMessage(ChatRole.Assistant, [toolCallContent]);
        return new AgentResponse([responseMessage]);
    }

    public static InformationRequestDetails Create()
    {
        var informationRequest =
            new InformationRequestDetails(
                "Travel Plan Information is missing.",
                ["Origin", "Destination", "StartDate", "EndDate"]);

        return informationRequest;
    }
}