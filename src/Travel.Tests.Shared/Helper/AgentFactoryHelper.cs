using Agents.Services;
using Infrastructure.Repository;
using Infrastructure.Settings;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Tests.Shared.Settings;
using Travel.Workflows.Infrastructure;
using Travel.Workflows.Interfaces;
using Travel.Workflows.Services;

namespace Travel.Tests.Shared.Helper;

public static  class AgentFactoryHelper
{
    private const string PlanningYaml = "planning.yaml";
    private const string ExtractingYaml = "extracting.yaml";

    public static IWorkflowFactory Create()
    {
        var mockFactory = new Mock<IWorkflowFactory>();

        var repo = new InMemoryCheckpointRepository();
        var sessionRepo = new InMemoryWorkflowSessionRepository();

        var agentProvider = new AgentProvider(
            AgentHelper.CreateAgentFactory(),
            AgentHelper.CreateAgentTemplateRepository());

        mockFactory.Setup(x => x.Create())
            .ReturnsAsync(() => new TravelWorkflowService(repo, sessionRepo, agentProvider));

        return mockFactory.Object;
    }

    public static IWorkflowFactory CreateWithFileRepositories()
    {
        var mockFactory = new Mock<IWorkflowFactory>();

        var fileStorageSettings = Options.Create(new FileStorageSettings
        {
            AgentTemplateFolder = "Templates",
            AgentThreadFolder = "Threads",
            CheckpointFolder = "Checkpoints",
            SessionFolder = "Sessions"
        });

        var mockFileLogger = new Mock<ILogger<FileRepository>>();
        var fileRepository = new FileRepository(mockFileLogger.Object);

        var checkpointRepo = new CheckpointRepository(fileRepository, fileStorageSettings);
        var sessionRepo = new WorkflowSessionRepository(fileRepository, fileStorageSettings);

        var agentProvider = new AgentProvider(
            AgentHelper.CreateAgentFactory(),
            AgentHelper.CreateAgentTemplateRepository());

        mockFactory.Setup(x => x.Create())
            .ReturnsAsync(() => new TravelWorkflowService(checkpointRepo, sessionRepo, agentProvider));

        return mockFactory.Object;
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

        var arguments = new Dictionary<string, object?>();

        if (!string.IsNullOrEmpty(agentCreateMeta.ArgumentsKey) && agentCreateMeta.Arguments != null)
        {
            var requestInfoElement = System.Text.Json.JsonSerializer.SerializeToElement(agentCreateMeta.Arguments);

            arguments.Add(agentCreateMeta.ArgumentsKey, requestInfoElement);
        }

        var functionCallContent = new FunctionCallContent(
            callId: $"call_{Guid.NewGuid()}",
            name: agentCreateMeta.Name,
            arguments: arguments
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

    public class AgentCreateMeta(AgentType agentType, string name, string? argumentsKey = null, object? arguments = null)
    {
        public AgentType AgentType { get; } = agentType;
        public string Name { get; } = name;

        public string? ArgumentsKey { get; } = argumentsKey;

        public object? Arguments { get; } = arguments;
    }
}