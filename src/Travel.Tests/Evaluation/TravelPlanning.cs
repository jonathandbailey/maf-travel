using FluentAssertions;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Agents.Services;
using Travel.Tests.Common;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Services;

namespace Travel.Tests.Evaluation;

public class TravelPlanning
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
    public async Task Test()
    {
        var travelPlanService = new Mock<ITravelPlanService>();
        
        travelPlanService.Setup(x => x.GetTravelPlanAsync()).ReturnsAsync(new TravelPlanDto());
     
        var agentTemplateRepository = AgentHelper.CreateAgentTemplateRepository();

        var agentFactory = AgentHelper.CreateAgentFactory();

        var agentProvider = new AgentProvider(agentFactory, agentTemplateRepository);

        var workflowService = new TravelWorkflowService(travelPlanService.Object, agentProvider);

        var request = new TravelWorkflowRequest(new ChatMessage(ChatRole.User, _firstMessage));

        var events = await workflowService.WatchStreamAsync(request).ToListAsync();

        events.Should().ShouldHaveType<TravelPlanUpdateEvent>();

        events.Should().ShouldHaveType<RequestInfoEvent>();

        var checkPoint = events.GetCheckpointInfo();

        checkPoint.Should().NotBeNull();

        workflowService = new TravelWorkflowService(travelPlanService.Object, agentProvider);

        request = new TravelWorkflowRequest(new ChatMessage(ChatRole.User, _secondMessage));

        events = await workflowService.WatchStreamAsync(request).ToListAsync();

        events.Should().ShouldHaveType<TravelPlanUpdateEvent>();
      
        events.Should().ShouldHaveType<TravelPlanningCompleteEvent>();
    }
}