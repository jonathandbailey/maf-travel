using Microsoft.Agents.AI.Workflows;
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
    private readonly DateTime _departureDate = new(2026, 5, 1);

    [Fact]
    public async Task Test()
    {
        var travelPlanService = new Mock<ITravelPlanService>();

        var events = new List<WorkflowEvent>();

        var agentTemplateRepository = AgentHelper.CreateAgentTemplateRepository();

        var agentFactory = AgentHelper.CreateAgentFactory();

        var agentProvider = new AgentProvider(agentFactory, agentTemplateRepository);

        var workflowService = new TravelWorkflowService(agentProvider);

        var request = new TravelWorkflowRequest($"I want to plan a trip from {Origin} to {Destination} on the {_departureDate:dd.MM.yyyy}, for {NumberOfTravelers} people.");

        await foreach (var evt in workflowService.WatchStreamAsync(request))
        {
            events.Add(evt);

            switch (evt)
            {
                case TravelPlanUpdateEvent travelPlanUpdateEvent:
                    break;
            }
        }

        travelPlanService.Verify(x => x.Update(It.IsAny<TravelPlanDto>()), Times.Once);

        Assert.Contains(events, @event => @event is TravelPlanUpdateEvent);
        Assert.Contains(events, @event => @event is RequestInfoEvent);
    }
}