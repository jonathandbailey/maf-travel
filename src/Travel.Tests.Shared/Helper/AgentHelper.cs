using Agents.Middleware;
using Agents.Services;
using Agents.Settings;
using Infrastructure.Repository;
using Infrastructure.Settings;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Travel.Tests.Shared.Settings;

namespace Travel.Tests.Shared.Helper;

public static class AgentHelper
{
    public static async Task<AIAgent> Create(string templateName, List<AITool>? tools = null)
    {
        var templateRepository = CreateAgentTemplateRepository();
    
        var agentTemplate = await templateRepository.LoadAsync(templateName);

        var agentFactory = CreateAgentFactory();

        var agent = await agentFactory.Create(agentTemplate, tools);

        return agent;
    }

    public static IAgentTemplateRepository CreateAgentTemplateRepository()
    {
        var fileStorageSettings = Options.Create(new FileStorageSettings
        {
            RootFolder = "Travel",
            AgentTemplateFolder = "Templates",
            AgentThreadFolder = "Threads",
            CheckpointFolder = "Checkpoints",
            SessionFolder = "Sessions"
        });

        var mockFileLogger = new Mock<ILogger<FileRepository>>();
        var fileRepository = new FileRepository(mockFileLogger.Object);

        var mockTemplateLogger = new Mock<ILogger<AgentTemplateRepository>>();
        var templateRepository = new AgentTemplateRepository(fileRepository, fileStorageSettings, mockTemplateLogger.Object);

        return templateRepository;
    }

    public static async Task<AIAgent> CreateWithFileThread(string templateName, List<AITool>? tools = null)
    {
        var templateRepository = CreateAgentTemplateRepository();
        var agentTemplate = await templateRepository.LoadAsync(templateName);
        var agentFactory = CreateAgentFactory();
        var agent = await agentFactory.Create(agentTemplate, tools);

        var fileStorageSettings = Options.Create(new FileStorageSettings
        {
            RootFolder = "Travel",
            AgentTemplateFolder = "Templates",
            AgentThreadFolder = "Threads",
            CheckpointFolder = "Checkpoints",
            SessionFolder = "Sessions"
        });

        var mockFileLogger = new Mock<ILogger<FileRepository>>();
        var fileRepository = new FileRepository(mockFileLogger.Object);
        var mockThreadRepoLogger = new Mock<ILogger<AgentThreadRepository>>();
        var agentThreadRepository = new AgentThreadRepository(fileRepository, fileStorageSettings, mockThreadRepoLogger.Object);

        var mockThreadLogger = new Mock<ILogger<IAgentAgUiMiddleware>>();
        var agentThreadMiddleware = new AgentThreadMiddleware(agentThreadRepository, mockThreadLogger.Object);

        var agentWithFileThread = agent.AsBuilder()
            .Use(runFunc: agentThreadMiddleware.RunAsync, runStreamingFunc: agentThreadMiddleware.RunStreamingAsync)
            .Build();

        return agentWithFileThread;
    }

    public static IAgentFactory CreateAgentFactory()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<AspireDashboardSettings>()
            .Build();

        var languageModelSettings = Options.Create(new LanguageModelSettings
        {
            DeploymentName = configuration["LanguageModelSettings:DeploymentName"] ?? string.Empty,
            EndPoint = configuration["LanguageModelSettings:EndPoint"] ?? string.Empty,
        });

        var mockMiddlewareFactory = new Mock<IAgentMiddlewareFactory>();

        var agentFactory = new AgentFactory(
            languageModelSettings,
            mockMiddlewareFactory.Object);

        return agentFactory;
    }
}