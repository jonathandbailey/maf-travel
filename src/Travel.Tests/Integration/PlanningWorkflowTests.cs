using Microsoft.Agents.AI.Workflows;
using Moq;
using Travel.Agents.Services;
using Travel.Tests.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Services;

namespace Travel.Tests.Integration;

public class PlanningWorkflowTests
{
    private const string Origin = "Zurich";
    private const string Destination = "Paris";
    private const int NumberOfTravelers = 2;
    private readonly DateTime _departureDate = new(2026, 5, 1);
    private readonly DateTime _returnDate = new(2026, 6, 15);

    [Fact]
    public async Task ShouldResumeFromCheckpointAndFinalize_WhenInformationRequestIsFulfilled()
    {
        var informationRequest = TestHelper.CreateInformationRequest();
        var travelUpdateRequest = new TravelPlanDto(Origin, Destination, _departureDate, null, NumberOfTravelers);

        var travelPlanService = new Mock<ITravelPlanService>();

        var agentProvider = new Mock<IAgentProvider>();

        var extractingAgent = new FakeAgent().UpdateTravelPlan(travelUpdateRequest);

        var planningAgent = new FakeAgent().InformationRequest(informationRequest);


        agentProvider.Setup(x => x.CreateAsync(AgentType.Extracting))
            .ReturnsAsync(extractingAgent);

        agentProvider.Setup(x => x.CreateAsync(AgentType.Planning))
            .ReturnsAsync(planningAgent);  


        var workflowService = new TravelWorkflowService(agentProvider.Object, travelPlanService.Object);
   
        var request = new TravelWorkflowRequest($"I want to plan a trip from {Origin} to {Destination} on the {_departureDate:dd.MM.yyyy}, for {NumberOfTravelers} people.");

        var events = new List<WorkflowEvent>();

        var _checkpointInfo = default(CheckpointInfo);

        await foreach (var evt in workflowService.WatchStreamAsync(request))
        {
            events.Add(evt);

            if (evt is SuperStepCompletedEvent superStepCompletedEvt)
            {
                var checkpoint = superStepCompletedEvt.CompletionInfo!.Checkpoint;

                if (checkpoint != null)
                {
                    _checkpointInfo = checkpoint;
                }
            }

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

        workflowService = new TravelWorkflowService(agentProvider.Object, travelPlanService.Object);
     
        events.Clear();
    
        request = new TravelWorkflowRequest($"My return date is {_returnDate:dd.MM.yyyy}", _checkpointInfo);

        travelUpdateRequest = new TravelPlanDto(EndDate: _returnDate);

        extractingAgent.UpdateTravelPlan(travelUpdateRequest);
    
        await foreach (var evt in workflowService.WatchStreamAsync(request))
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

