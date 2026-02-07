using Agents.Repository;
using Agents.Services;
using Agents.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace Travel.Workflows.Tests.Integration
{
    public class UnitTest1
    {
        [Fact]
        public async Task TestPlanningAgentAndFactory()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<UnitTest1>()
                .Build();

            var templateRepository = new AgentTemplateRepository();
            
            var languageModelSettings = Options.Create(new LanguageModelSettings
            {
                DeploymentName = configuration["LanguageModelSettings:DeploymentName"] ?? string.Empty,
                EndPoint = configuration["LanguageModelSettings:EndPoint"] ?? string.Empty,
                ApiKey = configuration["LanguageModelSettings:ApiKey"] ?? string.Empty
            });

            var mockMiddlewareFactory = new Mock<IAgentMiddlewareFactory>();
            
            var agentFactory = new AgentFactory(
                templateRepository,
                languageModelSettings,
                mockMiddlewareFactory.Object);

            // Act
            var agent = await agentFactory.Create("planning_agent");

            // Assert
            Assert.NotNull(agent);
            Assert.Equal("planning_agent", agent.Name);
        }
    }
}
