using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Agents.Services;
using Travel.Tests.Helpers;
using Travel.Workflows;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Services;

namespace Travel.Tests.Evaluation;

public class TravelPlanning
{
    private const string Origin = "Zurich";
    private const string Destination = "Paris";
    private const int NumberOfTravelers = 2;
    private readonly DateTime _departureDate = new DateTime(2026, 5, 1);

    [Fact]
    public async Task Test()
    {
        var extractingAgent = await AgentHelper.Create("extracting.yaml", ExtractingTools.GetDeclarationOnlyTools());
        var planningAgent = await AgentHelper.Create("planning.yaml", PlanningTools.GetDeclarationOnlyTools());

        var travelPlanService = new Mock<ITravelPlanService>();

        var workflowFactory = new WorkflowFactory(travelPlanService.Object);

        var workflow = workflowFactory.Build(planningAgent, extractingAgent);

        var checkpointManager = CheckpointManager.Default;

        var travelPlanningWorkflow = new TravelPlanningWorkflow();

        var message = new ChatMessage(ChatRole.User, $"I want to plan a trip from {Origin} to {Destination} on the {_departureDate:dd.MM.yyyy}, for {NumberOfTravelers} people.");

        var events = new List<WorkflowEvent>();

        await foreach (var evt in travelPlanningWorkflow.WatchStreamAsync(workflow, checkpointManager, message))
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