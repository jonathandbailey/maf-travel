using Microsoft.Agents.AI;
using Agents.Extensions;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Workflows.Tests.Integration.Helper;

namespace Travel.Workflows.Tests.Integration
{
    public class PlanningAgentIntegrationTests
    {
        [Fact]
        public async Task TestPlanningAgentAndFactory()
        {
            var threadId = Guid.NewGuid().ToString();

            var observation = MessageHelper.CreateObservation()
                .WithContext("I want to plan a trip to Paris on the 1st of May, 2026")
                .WithDestination("Paris")
                .WithStartDate("2026-05-01")
                .Build();

            var agent = await AgentHelper.Create("planning.yaml", PlanningTools.GetDeclarationOnlyTools());

            var message = MessageHelper.CreateObservationMessage(observation, new TravelPlanDto());

            var agentRunOptions = new ChatClientAgentRunOptions();

            agentRunOptions.AddThreadId(threadId);

            var response = await agent.RunAsync(message, options: agentRunOptions, cancellationToken: CancellationToken.None);

            ResponseHelper.ValidateFunctionCalls(response)
                .ShouldHaveCallCount(2)
                .ShouldContainCall("UpdateTravelPlan")
                    .WithDestination("Paris")
                    .WithStartDate(new DateTime(2026, 5, 1))
                    .And()
                .ShouldContainCall("RequestInformation")
                    .WithArgument("question");
        
        

            observation = MessageHelper.CreateObservation()
                .WithContext("We are returning on the 27.05.2026, and we are 4 travellers.")
                .WithNumberOfTravellers(4)
                .WithEndDate("2026-05-27")
                .Build();

            var travelPlan = new TravelPlanDto(null, "Paris", new DateTime(2026, 5, 1), null, null);

            message = MessageHelper.CreateObservationMessage(observation, travelPlan);

            agentRunOptions = new ChatClientAgentRunOptions();

            agentRunOptions.AddThreadId(threadId);

            response = await agent.RunAsync(message, options: agentRunOptions, cancellationToken: CancellationToken.None);
        }

    }
}
