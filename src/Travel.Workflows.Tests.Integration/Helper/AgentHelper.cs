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
using Travel.Workflows.Tests.Integration.Agents;

namespace Travel.Workflows.Tests.Integration.Helper;

public static class AgentHelper
{
    public static async Task<AIAgent> Create(string templateName, List<AITool>? tools = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<PlanningAgent>()
            .Build();

        var fileStorageSettings = Options.Create(new FileStorageSettings
        {
            AgentTemplateFolder = "Templates",
            AgentThreadFolder = "Threads"
        });

        var mockLogger = new Mock<ILogger<AgentTemplateRepository>>();

        var templateRepository = new AgentTemplateRepository(mockLogger.Object, fileStorageSettings);
    
        var agentTemplate = await templateRepository.LoadAsync(templateName);

        var languageModelSettings = Options.Create(new LanguageModelSettings
        {
            DeploymentName = configuration["LanguageModelSettings:DeploymentName"] ?? string.Empty,
            EndPoint = configuration["LanguageModelSettings:EndPoint"] ?? string.Empty,
        });

        var mockMiddlewareFactory = new Mock<IAgentMiddlewareFactory>();
  
        var agentFactory = new AgentFactory(
            languageModelSettings,
            mockMiddlewareFactory.Object);

        var agent = await agentFactory.Create(agentTemplate, tools);

        return agent;
    }
}