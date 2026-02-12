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
    private static readonly DateTime DepartureDate = new(2026, 5, 1);
    private static readonly DateTime ReturnDate = new(2026, 6, 15);

    private readonly string _message =
        $"I want to plan a trip from {Origin} to {Destination} on the {DepartureDate:dd.MM.yyyy}, for {NumberOfTravelers} people.";

    [Fact]
    public async Task PlanningWorkflow_ShouldUpdatePlanAndRequestionInformation_WhenIncompletePlanProvided()
    {
        var informationRequest = TestHelper.CreateInformationRequest();
        var travelUpdateRequest = new TravelPlanDto(Origin, Destination, DepartureDate, null, NumberOfTravelers);

        var agentProvider = new AgentScenarioBuilder()
            .WithExtractor(travelUpdateRequest)
            .WithPlanner(informationRequest)
            .BuildProvider();

        var workflowService = new TravelWorkflowService(agentProvider);

        var request = new TravelWorkflowRequest(_message);

        var events = await workflowService.WatchStreamAsync(request).ToListAsync();
  
        Assert.Contains(events, @event => @event is TravelPlanUpdateEvent);
        Assert.Contains(events, @event => @event is RequestInfoEvent);

        var travelPlanUpdateEvent = events.OfType<TravelPlanUpdateEvent>().FirstOrDefault();

        Assert.NotNull(travelPlanUpdateEvent);

        travelPlanUpdateEvent.MatchesAgentFunctionCallResponse(travelUpdateRequest);

        var requestInfoEvent = events.OfType<RequestInfoEvent>().FirstOrDefault();

        Assert.NotNull(requestInfoEvent);

        requestInfoEvent.MatchesAgentFunctionCallResponse(informationRequest);
    }


    [Fact]
    public async Task PlanningWorkflow_ShouldResumeFromCheckpointAndFinalize_WhenInformationRequestIsFulfilled()
    {
        var informationRequest = TestHelper.CreateInformationRequest();
        var travelUpdateRequest = new TravelPlanDto(Origin, Destination, DepartureDate, null, NumberOfTravelers);

        var travelPlanService = new Mock<ITravelPlanService>();

        var agentProvider = new Mock<IAgentProvider>();

        var extractingAgent = new FakeAgent().UpdateTravelPlan(travelUpdateRequest);

        var planningAgent = new FakeAgent().InformationRequest(informationRequest);


        agentProvider.Setup(x => x.CreateAsync(AgentType.Extracting))
            .ReturnsAsync(extractingAgent);

        agentProvider.Setup(x => x.CreateAsync(AgentType.Planning))
            .ReturnsAsync(planningAgent);  


        var workflowService = new TravelWorkflowService(agentProvider.Object);
   
        var request = new TravelWorkflowRequest($"I want to plan a trip from {Origin} to {Destination} on the {DepartureDate:dd.MM.yyyy}, for {NumberOfTravelers} people.");

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

        workflowService = new TravelWorkflowService(agentProvider.Object);
     
        events.Clear();
    
        request = new TravelWorkflowRequest($"My return date is {ReturnDate:dd.MM.yyyy}", _checkpointInfo);

        travelUpdateRequest = new TravelPlanDto(EndDate: ReturnDate);

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

