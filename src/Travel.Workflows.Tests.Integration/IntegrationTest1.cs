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


            var observation = new Dictionary<string, object>()
            {
                {"context", "I want to plan a trip to Paris on the 1st of May, 2026"},
                {"destination", "Paris"},
                {"startDate", "2026-05-01"}

            };

            var agent = await agentFactory.Create("planning_agent_ex", tools: PlanningTools.GetDeclarationOnlyTools());

            //agentFactory.UseMiddleware(agent, "agent-thread");

            var serialized = JsonSerializer.Serialize(new TravelPlanDto(null, null, null, null, null));

            var serializedObservation = JsonSerializer.Serialize(observation);

            var template = $"Observation: {serializedObservation} \nTravelPlanSummary : {serialized}";

            var message = new ChatMessage(ChatRole.User, template);

            var agentRunOptions = new ChatClientAgentRunOptions();

            agentRunOptions.AddThreadId(threadId);

            var response = await agent.RunAsync(message, options: agentRunOptions, cancellationToken: CancellationToken.None);

            observation = new Dictionary<string, object>()
            {
                {"context", "We are returning on the 27.05.2026, and we are 4 travellers."},
                {"numberOfTravellers", 4},
                {"endDate", "2026-05-27"}

            };

            serializedObservation = JsonSerializer.Serialize(observation);

            serialized = JsonSerializer.Serialize(new TravelPlanDto(null, "Paris", new DateTime(2026, 5, 1), null, null));

            template = $"Observation: {serializedObservation} \nTravelPlanSummary : {serialized}";

            message = new ChatMessage(ChatRole.User, template);

            agentRunOptions = new ChatClientAgentRunOptions();

            agentRunOptions.AddThreadId(threadId);

            response = await agent.RunAsync(message, options: agentRunOptions, cancellationToken: CancellationToken.None);
        }

    }
}
