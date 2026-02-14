using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Agents.Dto;
using Travel.Agents.Services;

namespace Travel.Tests.Common;

public static  class AgentFactoryHelper
{
    private const string PlanningYaml = "planning.yaml";

    public static async Task<AIAgent> CreateMockPlanningAgent()
    {
        var mockChatClient = new Mock<IChatClient>();

        var requestInfoDto = new RequestInformationDto(
            Message: "Please provide the missing information",
            Thought: "Need to request missing travel information",
            RequiredInputs: ["Origin", "ReturnDate"]
        );

        var requestInfoElement = System.Text.Json.JsonSerializer.SerializeToElement(requestInfoDto);

        var functionCallContent = new FunctionCallContent(
            callId: "call_123",
            name: "request_information",
            arguments: new Dictionary<string, object?>
            {
                ["request"] = requestInfoElement
            }
        );

        var mockMiddlewareFactory = new Mock<IAgentMiddlewareFactory>();

        var responseMessage = new ChatMessage(ChatRole.Assistant, [functionCallContent]);
        var finalResponse = new ChatMessage(ChatRole.Assistant, "Information request sent successfully.");

        mockChatClient
            .SetupSequence(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(responseMessage))
            .ReturnsAsync(new ChatResponse(finalResponse));

        var templateRepository = InfrastructureHelper.Create();

        var agentFactory = new AgentFactory(SettingsHelper.GetLanguageModelSettings(), mockMiddlewareFactory.Object);

        var template = await templateRepository.LoadAsync(PlanningYaml);

        var agent = await agentFactory.Create(mockChatClient.Object, template, PlanningTools.GetDeclarationOnlyTools());

        return agent;
    }
}