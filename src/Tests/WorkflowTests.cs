using Application.Agents;
using Application.Workflows.Conversations;
using Application.Workflows.Conversations.Dto;
using Microsoft.Extensions.AI;
using Moq;

namespace Tests;

public class WorkflowTests
{
    [Fact]
    public async Task Execute_WhenActAgentRequestsUserInput_ShouldReturnUserInputRequiredState()
    {
        var reasonAgent = new Mock<IAgent>();

        var actAgent = new Mock<IAgent>();
   
        reasonAgent.SetupAgentResponse(Data.ReasonTripToParisDeparturePointRequired);
        actAgent.SetupAgentResponse(Data.ActAgentDepartureCityResponse);
  
        var workFlow = new Workflow(reasonAgent.Object, actAgent.Object);

        var response = await workFlow.Execute(new ChatMessage(ChatRole.User, Data.PlanTripToParisUserRequest));

        Assert.NotNull(response);
        Assert.Equal(WorkflowResponseState.UserInputRequired, response.State);
        Assert.Equal(Data.ActAgentDepartureCityUserResponse, response.Message);
    }
}