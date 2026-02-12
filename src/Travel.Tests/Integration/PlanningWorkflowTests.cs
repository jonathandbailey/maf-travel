using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
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

    private readonly string _firstMessage =
        $"I want to plan a trip from {Origin} to {Destination} on the {DepartureDate:dd.MM.yyyy}, for {NumberOfTravelers} people.";

    private readonly string _secondMessage = $"My return date is {ReturnDate:dd.MM.yyyy}";

    [Fact]
    public async Task PlanningWorkflow_ShouldUpdatePlanAndRequestionInformation_WhenIncompletePlanProvided()
    {
        var informationRequest = TestHelper.CreateInformationRequest();
        var travelUpdateRequest = new TravelPlanDto(Origin, Destination, DepartureDate, null, NumberOfTravelers);

        var travelPlanService = new Mock<ITravelPlanService>();

        travelPlanService.Setup(x=> x.GetTravelPlanAsync()).ReturnsAsync(new TravelPlanDto());

        var agentProvider = new AgentScenarioBuilder()
            .WithExtractor(travelUpdateRequest)
            .WithPlanner(informationRequest)
            .BuildProvider();

        var workflowService = new TravelWorkflowService(travelPlanService.Object, agentProvider);

        var request = new TravelWorkflowRequest(new ChatMessage(ChatRole.User, _firstMessage));

        var events = await workflowService.WatchStreamAsync(request).ToListAsync();

        events.Should().ShouldHaveType<TravelPlanUpdateEvent>()
            .And.ShouldMatchFunctionCallResponse(travelUpdateRequest);

        events.Should().ShouldHaveType<RequestInfoEvent>()
            .And.ShouldMatchFunctionCallResponse(informationRequest);
    }


    [Fact]
    public async Task PlanningWorkflow_ShouldResumeFromCheckpointAndFinalize_WhenInformationRequestIsFulfilled()
    {
        var informationRequest = TestHelper.CreateInformationRequest();
        var travelUpdateRequest = new TravelPlanDto(Origin, Destination, DepartureDate, null, NumberOfTravelers);
        var travelPlanService = new Mock<ITravelPlanService>();
        travelPlanService.Setup(x => x.GetTravelPlanAsync()).ReturnsAsync(new TravelPlanDto());

        var agentProvider = new AgentScenarioBuilder()
            .WithExtractor(travelUpdateRequest)
            .WithPlanner(informationRequest)
            .BuildProvider();

        var workflowService = new TravelWorkflowService(travelPlanService.Object, agentProvider);
   
        var request = new TravelWorkflowRequest(new ChatMessage(ChatRole.User, _firstMessage));
     
        var events = await workflowService.WatchStreamAsync(request).ToListAsync();
   
        events.Should().ShouldHaveType<TravelPlanUpdateEvent>()
            .And.ShouldMatchFunctionCallResponse(travelUpdateRequest);

        events.Should().ShouldHaveType<RequestInfoEvent>()
            .And.ShouldMatchFunctionCallResponse(informationRequest);

        var checkpointInfo = events.GetCheckpointInfo();

        travelUpdateRequest = new TravelPlanDto(EndDate: ReturnDate);

        agentProvider = new AgentScenarioBuilder()
            .WithExtractor(travelUpdateRequest)
            .WithPlanner(informationRequest)
            .BuildProvider();

        workflowService = new TravelWorkflowService(travelPlanService.Object, agentProvider);
     
        events.Clear();
    
        request = new TravelWorkflowRequest(new ChatMessage(ChatRole.User, _secondMessage), checkpointInfo);

        travelUpdateRequest = new TravelPlanDto(EndDate: ReturnDate);

        events = await workflowService.WatchStreamAsync(request).ToListAsync();

        events.Should().ShouldHaveType<TravelPlanUpdateEvent>()
            .And.ShouldMatchFunctionCallResponse(travelUpdateRequest);

    }
}

