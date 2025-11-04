using Application.Agents;
using Application.Workflows.Conversations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Moq;

namespace Tests;

public class WorkflowTests
{
    [Fact]
    public async Task Test1()
    {
        var reasonAgent = new Mock<IAgent>();

        var actAgent = new Mock<IAgent>();

        var reasonAgentResponse = new AgentRunResponse(new ChatMessage(ChatRole.Assistant, "User want to plan a trip to Paris. Departure Point is required."));
        var actAgentResponse = new AgentRunResponse(new ChatMessage(ChatRole.Assistant, Data.AskUserDepartureCityResponse));


        reasonAgent.Setup(x => x.RunAsync(It.IsAny<IEnumerable<ChatMessage>>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reasonAgentResponse);

        actAgent.Setup(x => x.RunAsync(It.IsAny<IEnumerable<ChatMessage>>(), null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(actAgentResponse);

        var workFlow = new Workflow(reasonAgent.Object, actAgent.Object);

        await workFlow.Execute(new ChatMessage(ChatRole.User, "I want to plan a trip to Paris"));
    }
}