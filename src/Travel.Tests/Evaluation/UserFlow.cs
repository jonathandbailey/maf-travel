using Agents.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Travel.Agents.Services;
using Travel.Tests.Common;

namespace Travel.Tests.Evaluation
{
    public class UserFlow
    {
        [Fact]
        public async Task TravelExtractAgent_WhenProvidedWithNewObservationInformation_ShouldUpdateTravelPlan()
        {
            var threadId = Guid.NewGuid().ToString();

            var conversationAgent = await AgentHelper.Create("conversation.yaml", ConversationTools.GetDeclarationOnlyTools());

            var userMessage = new ChatMessage(ChatRole.User, "I want to plan a trip from Zurich to Paris on the 1st of May, 2026, for 2 people.");

            var agentRunOptions = new ChatClientAgentRunOptions();

            agentRunOptions.AddThreadId(threadId);

            var conversationResponse = await conversationAgent.RunAsync(userMessage, options: agentRunOptions, cancellationToken: CancellationToken.None);

            ResponseHelper.ValidateFunctionCalls(conversationResponse)
                .ShouldHaveCallCount(1)
                .ShouldContainCall("PlanTravel");

            var extractedMessage = ResponseHelper.GetFunctionCallArgument(conversationResponse, "PlanTravel", "message");

            var extractingAgent = await AgentHelper.Create("extracting.yaml", ExtractingTools.GetDeclarationOnlyTools());

            var message = new ChatMessage(ChatRole.User, extractedMessage!);

            agentRunOptions = new ChatClientAgentRunOptions();
                
            agentRunOptions.AddThreadId(threadId);

            var extractingResponse = await extractingAgent.RunAsync(message, options: agentRunOptions, cancellationToken: CancellationToken.None);

            var travelPlan = ResponseHelper.DeserializeTravelPlan(extractingResponse);

            ResponseHelper.ValidateFunctionCalls(extractingResponse)
                .ShouldHaveCallCount(1)
                .ShouldContainCall("UpdateTravelPlan")
                .WithDestination("Paris")
                .WithOrigin("Zurich")
                .WithNumberOfTravelers(2)
                .WithStartDate(new DateTime(2026, 5, 1));

            var agent = await AgentHelper.Create("planning.yaml", PlanningTools.GetDeclarationOnlyTools());

            message = MessageHelper.CreateTravelPlanMessage(travelPlan);

            agentRunOptions = new ChatClientAgentRunOptions();

            agentRunOptions.AddThreadId(threadId);

            var response = await agent.RunAsync(message, options: agentRunOptions, cancellationToken: CancellationToken.None);

            ResponseHelper.ValidateFunctionCalls(response)
                .ShouldHaveCallCount(1)
                .ShouldContainCall("RequestInformation")
                .WithArgument("request");
        }
    }
}
