using Agents.Services;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Workflows.Planning;
using Travel.Workflows.Planning.Dto;

namespace Travel.Workflows.Tests;

public class PlanningWorkflowTests
{
    private readonly Mock<IAgentFactory> _mockAgentFactory = new();

    [Fact]
    public async Task UpdateAndComplete()
    {
        var informationRequestDetails = TestHelper.Create();

        var agentResponse = TestHelper.CreateToolCallInformationRequestResponse();

        var updateTravelPlanResponse = TestHelper.CreateToolCallTravelPlanUpdateResponse();

        var finalizeTravelPlanResponse = TestHelper.CreateToolCallTravelPlanFinalizeResponse();

        TestHelper.SetupFakeAgent([agentResponse, updateTravelPlanResponse, finalizeTravelPlanResponse], _mockAgentFactory);

        var workflow = await TestHelper.CreateWorkflowAsync(_mockAgentFactory);
     
        Assert.NotNull(workflow);

        var inputMessage = new ChatMessage(ChatRole.User, "Update my travel plan to Tokyo");

        var travelPlanningWorkflow = new TravelPlanningWorkflow(workflow, CheckpointManager.Default);

        await foreach (var evt in travelPlanningWorkflow.Run(inputMessage))
        {
            if (evt is RequestInfoEvent requestInfoEvent)
            {
                var data = requestInfoEvent.Data as ExternalRequest;

                Assert.NotNull(data);

                var request = data.Data.AsType(typeof(InformationRequest)) as InformationRequest;

                Assert.NotNull(request);

                Assert.Equal(request.Context, informationRequestDetails.Context);
                Assert.Equal(request.Entities, informationRequestDetails.Entities);

                break;
            }
        }

        var workflow2 = await TestHelper.CreateWorkflowAsync(_mockAgentFactory);

        var travelPlanningWorkflow2 = new TravelPlanningWorkflow(workflow2, CheckpointManager.Default, travelPlanningWorkflow.CheckpointInfo, travelPlanningWorkflow.State);


        await foreach (var evt in travelPlanningWorkflow2.Run(inputMessage))
        {
            
        }
    }


    [Fact]
    public async Task ShouldPublishRequestInfoEventWithCorrectData_WhenPlannerRequestsInformation()
    {
        var informationRequestDetails = TestHelper.Create();
        
        var agentResponse = TestHelper.CreateToolCallInformationRequestResponse();
        
        TestHelper.SetupFakeAgent([agentResponse], _mockAgentFactory);

        var workflow = await TestHelper.CreateWorkflowAsync(_mockAgentFactory);
        
        Assert.NotNull(workflow);

        var inputMessage = new ChatMessage(ChatRole.User, "Update my travel plan to Tokyo");

        var travelPlanningWorkflow = new TravelPlanningWorkflow(workflow, CheckpointManager.Default);
       
        await foreach (var evt in travelPlanningWorkflow.Run(inputMessage))
        {
            if (evt is RequestInfoEvent requestInfoEvent)
            {
                var data = requestInfoEvent.Data as ExternalRequest;

                Assert.NotNull(data);

                var request = data.Data.AsType(typeof(InformationRequest)) as InformationRequest;

                Assert.NotNull(request);

                Assert.Equal(request.Context, informationRequestDetails.Context);
                Assert.Equal(request.Entities, informationRequestDetails.Entities);

                break;
            }
        }
    }
}

