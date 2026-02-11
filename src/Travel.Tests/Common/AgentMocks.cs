using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Moq;

namespace Travel.Tests.Common;

public static class AgentMocks
{
    public static IAgentFactory CreateAgentFactory(AIAgent fakeAgent)
    {
        var mockAgentFactory = new Mock<IAgentFactory>();

        mockAgentFactory
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ChatResponseFormat>(), It.IsAny<List<AITool>>()))
            .Returns(fakeAgent);

        return mockAgentFactory.Object;
    }
}