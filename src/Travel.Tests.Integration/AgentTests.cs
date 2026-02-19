using Agents.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Travel.Agents.Dto;
using Travel.Agents.Services;
using Travel.Tests.Shared;

namespace Travel.Tests.Integration
{
    
    
    public class AgentTests
    {
        private const string Origin = "Zurich";
        private const string Destination = "Paris";
        private const int NumberOfTravelers = 2;
        private static readonly DateTime DepartureDate = new(2026, 5, 1);

        private static readonly DateTime ReturnDate = new(2026, 6, 15);

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TravelPlanAgent_WhenProvidedWithCompletePlan_ShouldCompleteTheWorkflow()
        {
            var threadId = Guid.NewGuid().ToString();

            var agent = await AgentHelper.Create("planning.yaml",PlanningTools.GetDeclarationOnlyTools());

            var message = MessageHelper.CreateTravelPlanMessage(new TravelPlanDto(Origin, Destination, DepartureDate, ReturnDate, NumberOfTravelers));

            var agentRunOptions = new ChatClientAgentRunOptions();

            agentRunOptions.AddThreadId(threadId);

            var response = await agent.RunAsync(message, options: agentRunOptions, cancellationToken: CancellationToken.None);

            ResponseHelper.ValidateFunctionCalls(response)
                .ShouldHaveCallCount(1)
                .ShouldContainCall("PlanningComplete");
        }


        [Fact]
        [Trait("Category", "Integration")]
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
                .WithArgument("request")
                .WithRequiredInputs("Origin", "EndDate", "NumberOfTravelers");
        }


        [Fact]
        [Trait("Category", "Integration")]
        public async Task TravelConversation_WhenProvidedWithNewObservationInformation_ShouldUpdateTravelPlan()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<AgentTests>()
                .Build();

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

        [Fact]
        [Trait("Category", "Integration")]
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
                .ShouldContainCall(ExtractingTools.UpdateTravelPlanToolName)
                .WithDestination("Paris")
                .WithOrigin("Zurich")
                .WithNumberOfTravelers(2)
                .WithStartDate(new DateTime(2026, 5, 1));
        }
    }
}
