using Microsoft.Agents.AI;
using Agents.Extensions;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Workflows.Tests.Integration.Helper;

namespace Travel.Workflows.Tests.Integration
{
    public class PlanningAgent
    {
        [Fact]
        public async Task TravelPlanAgent_WhenProvidedWithNewObservationInformation_ShouldUpdateTravelPlan()
        {
            var threadId = Guid.NewGuid().ToString();
        
            var agent = await AgentHelper.Create("planning.yaml", PlanningTools.GetDeclarationOnlyTools());

            var message = MessageHelper.CreateTravelPlanMessage(new TravelPlanDto(null, "Paris", new DateTime(2026, 5, 1)));

            var agentRunOptions = new ChatClientAgentRunOptions();

            agentRunOptions.AddThreadId(threadId);

            var response = await agent.RunAsync(message, options: agentRunOptions, cancellationToken: CancellationToken.None);

            ResponseHelper.ValidateFunctionCalls(response)
                .ShouldHaveCallCount(1)
                .ShouldContainCall("RequestInformation")
                .WithArgument("message")
                .WithArgument("thought")
                .WithRequiredInputs("Origin", "EndDate", "NumberOfTravelers");
        }

    }
}
