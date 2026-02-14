
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Agents.Services;
using Travel.Tests.Common;
using Travel.Tests.Helper;
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

        var harness = new WorkflowMockTestHarness2();

        var meta = new List<AgentFactoryHelper.AgentCreateMeta>
        {
            new(AgentType.Planning, PlanningTools.RequestInformationToolName, "request", informationRequest ),
            new(AgentType.Extracting, ExtractingTools.UpdateTravelPlanToolName, "travelPlan", travelUpdateRequest)
        };


        var events = await harness.WatchStreamAsync(_firstMessage, meta);

        events.ShouldHaveEvent().ShouldHaveType<TravelPlanUpdateEvent>()
            .And.ShouldMatchFunctionCallResponse(travelUpdateRequest);

        events.ShouldHaveEvent().ShouldHaveType<RequestInfoEvent>()
            .And.ShouldMatchFunctionCallResponse(informationRequest);
    }


    [Fact]
    public async Task PlanningWorkflow_ShouldResumeFromCheckpointAndFinalize_WhenInformationRequestIsFulfilled()
    {
        var threadId = Guid.NewGuid();
        var checkpointRepository = new InMemoryCheckpointRepository();


        var informationRequest = TestHelper.CreateInformationRequest();
        var travelUpdateRequest = new TravelPlanDto(Origin, Destination, DepartureDate, null, NumberOfTravelers);
        var travelcompleteRequest = new TravelPlanDto(Origin, Destination, DepartureDate, ReturnDate, NumberOfTravelers);
        var travelPlanService = new Mock<ITravelPlanService>();
        
        travelPlanService.Setup(x => x.GetTravelPlanAsync()).ReturnsAsync(new TravelPlanDto());

        var agentProvider = new Mock<IAgentProvider>();

        agentProvider.Setup(x => x.CreateAsync(AgentType.Planning))
            .ReturnsAsync(await AgentFactoryHelper.CreateMockPlanningAgent(informationRequest));

        agentProvider.Setup(x => x.CreateAsync(AgentType.Extracting))
            .ReturnsAsync(await AgentFactoryHelper.CreateMockExtractorAgent(travelUpdateRequest));

        var workflowService = new TravelWorkflowService(travelPlanService.Object, checkpointRepository, agentProvider.Object);
   
        var request = new TravelWorkflowRequest(new ChatMessage(ChatRole.User, _firstMessage), threadId);
     
        var events = await workflowService.WatchStreamAsync(request).ToListAsync();
   
        events.ShouldHaveEvent().ShouldHaveType<TravelPlanUpdateEvent>()
            .And.ShouldMatchFunctionCallResponse(travelUpdateRequest);

        events.ShouldHaveEvent().ShouldHaveType<RequestInfoEvent>()
            .And.ShouldMatchFunctionCallResponse(informationRequest);

        var checkpointInfo = events.GetCheckpointInfo();

        travelUpdateRequest = new TravelPlanDto(endDate: ReturnDate);

        agentProvider = new Mock<IAgentProvider>();

        agentProvider.Setup(x => x.CreateAsync(AgentType.Planning))
            .ReturnsAsync(await AgentFactoryHelper.CreateMockPlanningAgent());

        agentProvider.Setup(x => x.CreateAsync(AgentType.Extracting))
            .ReturnsAsync(await AgentFactoryHelper.CreateMockExtractorAgent(travelUpdateRequest));

        workflowService = new TravelWorkflowService(travelPlanService.Object, checkpointRepository, agentProvider.Object);
     
        events.Clear();
    
        request = new TravelWorkflowRequest(new ChatMessage(ChatRole.User, _secondMessage), threadId, checkpointInfo);
      
        events = await workflowService.WatchStreamAsync(request).ToListAsync();

        events.ShouldHaveEvent().ShouldHaveType<TravelPlanUpdateEvent>()
            .And.ShouldMatchFunctionCallResponse(travelcompleteRequest);

        events.ShouldHaveEvent().ShouldHaveType<TravelPlanningCompleteEvent>("Events should contain type : TravelPlanningCompleteEvent");

    }
}

