using System.Text.Json;
using Agents.Repository;
using Agents.Services;
using Agents.Settings;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Agents.Extensions;
using Travel.Agents.Planning.Dto;
using Travel.Agents.Planning.Services;

namespace Travel.Workflows.Tests.Integration
{
    public class IntegrationTest1
    {
        [Fact]
        public async Task TestPlanningAgentAndFactory()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<IntegrationTest1>()
                .Build();

            var templateRepository = new AgentTemplateRepository();

            var languageModelSettings = Options.Create(new LanguageModelSettings
            {
                DeploymentName = configuration["LanguageModelSettings:DeploymentName"] ?? string.Empty,
                EndPoint = configuration["LanguageModelSettings:EndPoint"] ?? string.Empty,
            });

            var mockMiddlewareFactory = new Mock<IAgentMiddlewareFactory>();

            var threadId = Guid.NewGuid().ToString();

            var agentFactory = new AgentFactory(
                templateRepository,
                languageModelSettings,
                mockMiddlewareFactory.Object);

            var agent = await agentFactory.Create("planning_agent_ex", tools: PlanningTools.GetDeclarationOnlyTools());

            //agentFactory.UseMiddleware(agent, "agent-thread");

            var serialized = JsonSerializer.Serialize(new TravelPlanDto(null, null, null, null, null));

            var template = $"Observation: \nTravelPlanSummary : {serialized}";

            var message = new ChatMessage(ChatRole.User, template);

            var agentRunOptions = new ChatClientAgentRunOptions();

            agentRunOptions.AddThreadId(threadId);

            var response = await agent.RunAsync(message, options: agentRunOptions, cancellationToken: CancellationToken.None);

            
        }

    }
}
