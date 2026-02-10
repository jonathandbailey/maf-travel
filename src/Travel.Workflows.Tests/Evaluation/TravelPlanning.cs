using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Agents.Services;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Services;
using Travel.Workflows.Tests.Integration.Helper;

namespace Travel.Workflows.Tests.Evaluation;

public class TravelPlanning
{
    [Fact]
    public async Task Test()
    {
        var extractingAgent = await AgentHelper.Create("extracting.yaml", ExtractingTools.GetDeclarationOnlyTools());
        var planningAgent = await AgentHelper.Create("planning.yaml", PlanningTools.GetDeclarationOnlyTools());

        var travelPlanService = new Mock<ITravelPlanService>();

        var workflowFactory = new WorkflowFactory2(travelPlanService.Object);

        var workflow = workflowFactory.Build(planningAgent, extractingAgent);

        var checkpointManager = CheckpointManager.Default;

        var travelPlanningWorkflow = new TravelPlanningWorkflow();

        var message = new ChatMessage(ChatRole.User, "I want to plan a trip from Zurich to Paris on the 1st of May, 2026, for 2 people.");

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