using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Tests.Common;
using Travel.Workflows;
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

        var extractingAgent = new FakeAgent()
            .UpdateTravelPlan(travelUpdateRequest);

        var planningAgent = new FakeAgent()
            .InformationRequest(informationRequest);
      
        var travelPlanService = new Mock<ITravelPlanService>();
    
        var workflowFactory = new WorkflowFactory(travelPlanService.Object);

        var workflow = workflowFactory.Build(planningAgent, extractingAgent);

        var checkpointManager = CheckpointManager.Default;

        var inputMessage = new ChatMessage(ChatRole.User, $"I want to plan a trip from {Origin} to {Destination} on the {_departureDate:dd.MM.yyyy}, for {NumberOfTravelers} people.");

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

        workflowFactory = new WorkflowFactory(travelPlanService.Object);

        workflow = workflowFactory.Build(planningAgent, extractingAgent);

        travelPlanningWorkflow = new TravelPlanningWorkflow();
     
        events.Clear();

        var informationResponse = new ChatMessage(ChatRole.User, $"My return date is {_returnDate:dd.MM.yyyy}");

        travelUpdateRequest = new TravelPlanDto(EndDate: _returnDate);

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

