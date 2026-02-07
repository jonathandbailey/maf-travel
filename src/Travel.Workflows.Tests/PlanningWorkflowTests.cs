using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Travel.Workflows.Planning;
using Travel.Workflows.Planning.Dto;
using Travel.Workflows.Planning.Events;
using Travel.Workflows.Planning.Services;
using Travel.Workflows.Tests.Helpers;

namespace Travel.Workflows.Tests;

public class PlanningWorkflowTests
{
    [Fact]
    public async Task UpdateAndComplete()
    {
        var informationRequest = TestHelper.CreateInformationRequest();
        var travelUpdateRequest = TestHelper.CreateTravelUpdateRequest();

        var agent = new FakeAgent()
            .UpdateTravelPlan(travelUpdateRequest)
            .InformationRequest(informationRequest)
            .UpdateTravelPlan(travelUpdateRequest)
            .FinalizeTravelPlan();
     

        var agentFactory = AgentMocks.CreateAgentFactory(agent);

        var travelPlanService = new Mock<ITravelPlanService>();
    
        var workflowFactory = new WorkflowFactory(agentFactory, travelPlanService.Object);

        var workflow = await workflowFactory.Build();

        Assert.NotNull(workflow);

        var inputMessage = new ChatMessage(ChatRole.User, "Update my travel plan to Tokyo");

        var travelPlanningWorkflow = new TravelPlanningWorkflow(workflow, CheckpointManager.Default);

        var events = new List<WorkflowEvent>();

        await foreach (var evt in travelPlanningWorkflow.Run(inputMessage))
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

        Assert.Contains(events, @event => @event is TravelPlanUpdateEvent);
        Assert.Contains(events, @event => @event is RequestInfoEvent);

        workflowFactory = new WorkflowFactory(agentFactory, travelPlanService.Object);

        workflow = await workflowFactory.Build();

        travelPlanningWorkflow = new TravelPlanningWorkflow(workflow, CheckpointManager.Default, travelPlanningWorkflow.CheckpointInfo, travelPlanningWorkflow.State);
     
        events.Clear();

        await foreach (var evt in travelPlanningWorkflow.Run(inputMessage))
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


    [Fact]
    public async Task ShouldPublishRequestInfoEventWithCorrectData_WhenPlannerRequestsInformation()
    {
        var informationRequest = TestHelper.CreateInformationRequest();

        var agent = new FakeAgent().InformationRequest(informationRequest);
     
        var agentFactory = AgentMocks.CreateAgentFactory(agent);

        var workflowTools = new WorkflowTools();

        var travelPlanService = new Mock<ITravelPlanService>().Object;

        var workflowFactory = new WorkflowFactory(agentFactory, travelPlanService);
        
        var workflow =  await workflowFactory.Build();
     
        var inputMessage = new ChatMessage(ChatRole.User, "Update my travel plan to Tokyo");

        var travelPlanningWorkflow = new TravelPlanningWorkflow(workflow, CheckpointManager.Default);

        await foreach (var evt in travelPlanningWorkflow.Run(inputMessage))
        {
            if (evt is RequestInfoEvent requestInfoEvent)
            {
                requestInfoEvent.MatchesAgentFunctionCallResponse(informationRequest);

                break;
            }
        }
    }
}

