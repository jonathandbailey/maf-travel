using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Workflows.Dto;
using TravelPlanDto = Travel.Workflows.Dto.TravelPlanDto;


namespace Travel.Tests.Common;

public static  class AgentFactoryHelper
{
    private const string PlanningYaml = "planning.yaml";
    private const string ExtractingYaml = "extracting.yaml";

    public static async Task<AIAgent> CreateMockExtractorAgent(TravelPlanDto travelPlanDto)
    {
        var mockChatClient = new Mock<IChatClient>();

        var requestInfoElement = System.Text.Json.JsonSerializer.SerializeToElement(travelPlanDto);

        var arguments = new Dictionary<string, object?>
        {
            ["travelPlan"] = requestInfoElement
        };

        var functionCallContent = new FunctionCallContent(
            callId: "call_456",
            name: ExtractingTools.UpdateTravelPlanToolName,
            arguments: arguments);

        var mockMiddlewareFactory = new Mock<IAgentMiddlewareFactory>();

        var responseMessage = new ChatMessage(ChatRole.Assistant, [functionCallContent]);


        mockChatClient
            .SetupSequence(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(responseMessage));

        var templateRepository = InfrastructureHelper.Create();

        var agentFactory = new AgentFactory(SettingsHelper.GetLanguageModelSettings(), mockMiddlewareFactory.Object);

        var template = await templateRepository.LoadAsync(ExtractingYaml);

        var agent = await agentFactory.Create(mockChatClient.Object, template, ExtractingTools.GetDeclarationOnlyTools());

        return agent;
    }

    public static async Task<AIAgent> CreateMockPlanningAgent(RequestInformationDto requestInfoDto)
    {
        var mockChatClient = new Mock<IChatClient>( );

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


        mockChatClient
            .SetupSequence(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(responseMessage));
       
        var templateRepository = InfrastructureHelper.Create();

        var agentFactory = new AgentFactory(SettingsHelper.GetLanguageModelSettings(), mockMiddlewareFactory.Object);

        var template = await templateRepository.LoadAsync(PlanningYaml);

        var agent = await agentFactory.Create(mockChatClient.Object, template, PlanningTools.GetDeclarationOnlyTools());

        return agent;
    }

    public static async Task<AIAgent> CreateMockPlanningAgent()
    {
        var mockChatClient = new Mock<IChatClient>();

    
        var functionCallContent = new FunctionCallContent(
            callId: "call_193",
            name: "PlanningComplete"
        );

        var mockMiddlewareFactory = new Mock<IAgentMiddlewareFactory>();

        var responseMessage = new ChatMessage(ChatRole.Assistant, [functionCallContent]);


        mockChatClient
            .SetupSequence(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(responseMessage));

        var templateRepository = InfrastructureHelper.Create();

        var agentFactory = new AgentFactory(SettingsHelper.GetLanguageModelSettings(), mockMiddlewareFactory.Object);

        var template = await templateRepository.LoadAsync(PlanningYaml);

        var agent = await agentFactory.Create(mockChatClient.Object, template, PlanningTools.GetDeclarationOnlyTools());

        return agent;
    }


}