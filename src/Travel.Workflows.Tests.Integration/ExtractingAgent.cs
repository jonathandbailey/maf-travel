using Microsoft.Agents.AI;
using Agents.Extensions;
using Microsoft.Extensions.AI;
using Travel.Agents.Services;
using Travel.Workflows.Tests.Integration.Helper;

namespace Travel.Workflows.Tests.Integration
{
    public class ExtractingAgent
    {
        [Fact]
        public async Task TravelExtractAgent_WhenProvidedWithNewObservationInformation_ShouldUpdateTravelPlan()
        {
            var threadId = Guid.NewGuid().ToString();

            var agent = await AgentHelper.Create("extracting.yaml", ExtractingTools.GetDeclarationOnlyTools());

            var message = new ChatMessage(ChatRole.User, "I want to plan a trip from Zurich to Paris on the 1st of May, 2026, for 2 people.");

            var agentRunOptions = new ChatClientAgentRunOptions();

            agentRunOptions.AddThreadId(threadId);

            var response = await agent.RunAsync(message, options: agentRunOptions, cancellationToken: CancellationToken.None);

            ResponseHelper.ValidateFunctionCalls(response)
                .ShouldHaveCallCount(1)
                .ShouldContainCall("UpdateTravelPlan")
                .WithDestination("Paris")
                .WithOrigin("Zurich")
                .WithNumberOfTravelers(2)
                .WithStartDate(new DateTime(2026, 5, 1));
        }
    }
}
