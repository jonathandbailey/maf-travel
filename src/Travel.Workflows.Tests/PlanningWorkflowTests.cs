using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Workflows.Planning;
using Travel.Workflows.Planning.Services;
using Travel.Workflows.Tests.Helpers;

namespace Travel.Workflows.Tests;

public class PlanningWorkflowTests
{
    [Fact]
    public async Task UpdateAndComplete()
    {
        var informationRequest = TestHelper.CreateInformationRequest();

        var agent = new FakeAgent().
             ReturnsInformationRequestFunctionCall(informationRequest)
            .ReturnsUpdateTravelPlanFunctionCall()
            .ReturnsFinalizeTravelPlanFunctionCall();


       
        var updateTravelPlanTool = AIFunctionFactory.Create(
            (string message) => $"The travel plan has been updated with the following information: {message}",
            name: "update_travel_plan",
            description: "Update the travel plan");

        var agentFactory = AgentMocks.CreateAgentFactory(agent);

        var travelPlanService = new Mock<ITravelPlanService>().Object;
    
        var workflowFactory = new WorkflowFactory(agentFactory, travelPlanService);

        var workflow = await workflowFactory.Build();

        Assert.NotNull(workflow);

        var inputMessage = new ChatMessage(ChatRole.User, "Update my travel plan to Tokyo");

        var travelPlanningWorkflow = new TravelPlanningWorkflow(workflow, CheckpointManager.Default);

        await foreach (var evt in travelPlanningWorkflow.Run(inputMessage))
        {
            if (evt is RequestInfoEvent requestInfoEvent)
            {
                requestInfoEvent.MatchesAgentFunctionCallResponse(informationRequest);
            }
        }

        workflowFactory = new WorkflowFactory(agentFactory, travelPlanService);

        workflow = await workflowFactory.Build();

        travelPlanningWorkflow = new TravelPlanningWorkflow(workflow, CheckpointManager.Default, travelPlanningWorkflow.CheckpointInfo, travelPlanningWorkflow.State);


        await foreach (var evt in travelPlanningWorkflow.Run(inputMessage))
        {
            
        }
    }


    [Fact]
    public async Task ShouldPublishRequestInfoEventWithCorrectData_WhenPlannerRequestsInformation()
    {
        var informationRequest = TestHelper.CreateInformationRequest();

        var agent = new FakeAgent().ReturnsInformationRequestFunctionCall(informationRequest);
     
        var agentFactory = AgentMocks.CreateAgentFactory(agent);

        var workflowTools = new WorkflowTools();

        var travelPlanService = new Mock<ITravelPlanService>().Object;

        var workflowFactory = new WorkflowFactory(agentFactory, travelPlanService);
        
        var workflow =  await workflowFactory.Build();
     
        var inputMessage = new ChatMessage(ChatRole.User, "Update my travel plan to Tokyo");

        var travelPlanningWorkflow = new TravelPlanningWorkflow(workflow, CheckpointManager.Default);

        await foreach (var evt in travelPlanningWorkflow.Run(inputMessage))
        {
            if (evt is RequestInfoEvent requestInfoEvent)
            {
                requestInfoEvent.MatchesAgentFunctionCallResponse(informationRequest);

                break;
            }
        }
    }
}

