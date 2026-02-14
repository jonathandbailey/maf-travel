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

    public static IChatClient CreateMockChatClient(AgentCreateMeta agentCreateMeta)
    {
        var mockChatClient = new Mock<IChatClient>();

        var requestInfoElement = System.Text.Json.JsonSerializer.SerializeToElement(agentCreateMeta.Arguments);

        var functionCallContent = new FunctionCallContent(
            callId: $"call_{Guid.NewGuid()}",
            name: agentCreateMeta.Name,
            arguments: new Dictionary<string, object?>
            {
                [agentCreateMeta.ArgumentsKey] = requestInfoElement
            }
        );

        var responseMessage = new ChatMessage(ChatRole.Assistant, [functionCallContent]);

        mockChatClient
            .SetupSequence(c => c.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(responseMessage));

        return mockChatClient.Object;
    }

    public static async Task<AIAgent> Create(AgentType type)
    {
        var mockMiddlewareFactory = new Mock<IAgentMiddlewareFactory>();

        var agentFactory = new AgentFactory(SettingsHelper.GetLanguageModelSettings(), mockMiddlewareFactory.Object);

        var templateRepository = InfrastructureHelper.Create();

        var agentProvider = new AgentProvider(agentFactory, templateRepository);

        return await agentProvider.CreateAsync(type);
    }

    public static async Task<AIAgent> Create(AgentType type, IChatClient chatClient)
    {
        var mockMiddlewareFactory = new Mock<IAgentMiddlewareFactory>();

        var agentFactory = new AgentFactory(SettingsHelper.GetLanguageModelSettings(), mockMiddlewareFactory.Object);

        var templateRepository = InfrastructureHelper.Create();

        var agentProvider = new AgentProvider(agentFactory, templateRepository);

        return await agentProvider.CreateAsync(type, chatClient);
    }

    public static async Task<AIAgent> CreateMockPlanningAgent(AgentCreateMeta agentCreateMeta)
    {
        var mockChatClient = new Mock<IChatClient>();

        var requestInfoElement = System.Text.Json.JsonSerializer.SerializeToElement(agentCreateMeta.Arguments);

        var functionCallContent = new FunctionCallContent(
            callId: $"call_{Guid.NewGuid()}",
            name: agentCreateMeta.Name,
            arguments: new Dictionary<string, object?>
            {
                [agentCreateMeta.ArgumentsKey] = requestInfoElement
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

    public class AgentCreateMeta(AgentType agentType, string name, string argumentsKey, object arguments)
    {
        public AgentType AgentType { get; } = agentType;
        public string Name { get; } = name;

        public string ArgumentsKey { get; } = argumentsKey;

        public object Arguments { get; } = arguments;
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