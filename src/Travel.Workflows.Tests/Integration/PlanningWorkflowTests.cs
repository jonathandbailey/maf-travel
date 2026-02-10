using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Services;
using Travel.Workflows.Tests.Helpers;

namespace Travel.Workflows.Tests.Integration;

public class PlanningWorkflowTests
{
    [Fact]
    public async Task ShouldResumeFromCheckpointAndFinalize_WhenInformationRequestIsFulfilled()
    {
        var informationRequest = TestHelper.CreateInformationRequest();
        var travelUpdateRequest = new TravelPlanDto("Zurich", "Paris", new DateTime(2026, 5, 1), null, 2);

        var extractingAgent = new FakeAgent()
            .UpdateTravelPlan(travelUpdateRequest);

        var planningAgent = new FakeAgent()
            .InformationRequest(informationRequest);
      
        var travelPlanService = new Mock<ITravelPlanService>();
    
        var workflowFactory = new WorkflowFactory2(travelPlanService.Object);

        var workflow = workflowFactory.Build(planningAgent, extractingAgent);

        var checkpointManager = CheckpointManager.Default;

        var inputMessage = new ChatMessage(ChatRole.User, "I want to plan a trip from Zurich to Paris on the 1st of May, 2026, for 2 people.");

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

        workflowFactory = new WorkflowFactory2(travelPlanService.Object);

        workflow = workflowFactory.Build(planningAgent, extractingAgent);

        travelPlanningWorkflow = new TravelPlanningWorkflow();
     
        events.Clear();

        var informationResponse = new ChatMessage(ChatRole.User, "My return date is June 15, 2026");

        travelUpdateRequest = new TravelPlanDto(EndDate: new DateTime(2026, 6, 15));


        extractingAgent.UpdateTravelPlan(travelUpdateRequest);

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
   
}

