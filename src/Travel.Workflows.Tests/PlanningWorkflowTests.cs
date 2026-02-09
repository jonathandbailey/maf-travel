using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Workflows.Planning;
using Travel.Workflows.Planning.Dto;
using Travel.Workflows.Planning.Events;
using Travel.Workflows.Planning.Services;
using Travel.Workflows.Tests.Helpers;

namespace Travel.Workflows.Tests;

public class PlanningWorkflowTests
{
    [Fact]
    public async Task ShouldResumeFromCheckpointAndFinalize_WhenInformationRequestIsFulfilled()
    {
        var informationRequest = TestHelper.CreateInformationRequest();
        var travelUpdateRequest = TestHelper.CreateTravelUpdateRequest();

        var agent = new FakeAgent()
            .UpdateTravelPlan(travelUpdateRequest)
            .InformationRequest(informationRequest)
            .UpdateTravelPlan(travelUpdateRequest)
            .FinalizeTravelPlan();
     

        var agentFactory = AgentMocks.CreateAgentFactory(agent);

        var travelPlanService = new Mock<ITravelPlanService>();
    
        var workflowFactory = new WorkflowFactory(agentFactory, travelPlanService.Object);

        var workflow = await workflowFactory.Build();

        var checkpointManager = CheckpointManager.Default;

        var inputMessage = new ChatMessage(ChatRole.User, "Update my travel plan to Tokyo");

        var travelPlanningWorkflow = new TravelPlanningWorkflow();

        var events = new List<WorkflowEvent>();

        await foreach (var evt in travelPlanningWorkflow.WatchStreamAsync(workflow, checkpointManager,  inputMessage))
        {
            events.Add(evt);

            switch (evt)
            {
                case TravelPlanUpdateEvent travelPlanUpdateEvent:
                    travelPlanUpdateEvent.MatchesAgentFunctionCallResponse(travelUpdateRequest);
                    break;
                case RequestInfoEvent requestInfoEvent:
                    requestInfoEvent.MatchesAgentFunctionCallResponse(informationRequest);
                    break;
            }
        }

        travelPlanService.Verify(x => x.Update(It.IsAny<TravelPlanDto>()), Times.Once);

        Assert.Contains(events, @event => @event is TravelPlanUpdateEvent);
        Assert.Contains(events, @event => @event is RequestInfoEvent);

        workflowFactory = new WorkflowFactory(agentFactory, travelPlanService.Object);

        workflow = await workflowFactory.Build();

        travelPlanningWorkflow = new TravelPlanningWorkflow();
     
        events.Clear();

        var informationResponse = new ChatMessage(ChatRole.User, "My travel dates are June 1-15, 2024");

        await foreach (var evt in travelPlanningWorkflow.WatchStreamAsync(workflow, checkpointManager, informationResponse))
        {
            events.Add(evt);

            switch (evt)
            {
                case TravelPlanUpdateEvent travelPlanUpdateEvent:
                    travelPlanUpdateEvent.MatchesAgentFunctionCallResponse(travelUpdateRequest);
                    break;
            }
        }

        Assert.Contains(events, @event => @event is TravelPlanUpdateEvent);
      
        travelPlanService.Verify(x => x.Update(It.IsAny<TravelPlanDto>()), Times.Exactly(2));
    }


    [Fact]
    public async Task ShouldPublishRequestInfoEventWithCorrectData_WhenPlannerRequestsInformation()
    {
        var informationRequest = TestHelper.CreateInformationRequest();

        var agent = new FakeAgent().InformationRequest(informationRequest);
     
        var agentFactory = AgentMocks.CreateAgentFactory(agent);

        var travelPlanService = new Mock<ITravelPlanService>().Object;

        var workflowFactory = new WorkflowFactory(agentFactory, travelPlanService);
        
        var workflow =  await workflowFactory.Build();
     
        var inputMessage = new ChatMessage(ChatRole.User, "Update my travel plan to Tokyo");

        var travelPlanningWorkflow = new TravelPlanningWorkflow();

        await foreach (var evt in travelPlanningWorkflow.WatchStreamAsync(workflow,CheckpointManager.Default, inputMessage))
        {
            if (evt is RequestInfoEvent requestInfoEvent)
            {
                requestInfoEvent.MatchesAgentFunctionCallResponse(informationRequest);

                break;
            }
        }
    }
}

