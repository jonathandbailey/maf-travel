using Agents.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Travel.Agents.Services;
using Travel.Workflows.Tests.Integration.Helper;

namespace Travel.Workflows.Tests.Evaluation
{
    public class ConversationAgent
    {
        [Fact]
        public async Task TravelExtractAgent_WhenProvidedWithNewObservationInformation_ShouldUpdateTravelPlan()
        {
            var threadId = Guid.NewGuid().ToString();

            var agent = await AgentHelper.Create("conversation.yaml", ConversationTools.GetDeclarationOnlyTools());

            var message = new ChatMessage(ChatRole.User, "I want to plan a trip from Zurich to Paris on the 1st of May, 2026, for 2 people.");

            var agentRunOptions = new ChatClientAgentRunOptions();

            agentRunOptions.AddThreadId(threadId);

            var response = await agent.RunAsync(message, options: agentRunOptions, cancellationToken: CancellationToken.None);

            ResponseHelper.ValidateFunctionCalls(response)
                .ShouldHaveCallCount(1)
                .ShouldContainCall("PlanTravel");
        }
    }
}
