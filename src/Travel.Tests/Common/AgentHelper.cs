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

namespace Travel.Tests.Common;

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
            AgentTemplateFolder = "Templates",
            AgentThreadFolder = "Threads",
            CheckpointFolder = "Checkpoints"
        });

        var mockLogger = new Mock<ILogger<AgentTemplateRepository>>();

        var templateRepository = new AgentTemplateRepository(mockLogger.Object, fileStorageSettings);

        return templateRepository;
    }

    public static IAgentFactory CreateAgentFactory()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Evaluation.Agents>()
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