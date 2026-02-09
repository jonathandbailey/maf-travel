using Agents.Services;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Workflows.Planning.Dto;
using Travel.Workflows.Planning.Nodes;

namespace Travel.Workflows.Planning.Services;

public class WorkflowFactory(IAgentFactory agentFactory,ITravelPlanService travelPlanService)
{
    public async Task<Workflow> Build()
    {
        var planningAgent = await agentFactory.Create("planning_agent");

        var planningNode = new PlanningNode(planningAgent);

        var executionNode = new ExecutionNode();
        var travelPlanNode = new TravelPlanNode(travelPlanService);
        var requestInformationNode = new RequestInformationNode();

        var finalizerNode = new FinalizerNode();

        var requestInformationPort = RequestPort.Create<InformationRequest, InformationResponse>("information");

        var builder = new WorkflowBuilder(planningNode);

        builder.AddEdge(planningNode, executionNode);
        builder.AddEdge(executionNode, planningNode);
        

        builder.AddEdge<FunctionCallContent>(
            source: executionNode, 
            target:travelPlanNode,
            condition: result => result is { Name: "update_travel_plan" });

        builder.AddEdge<FunctionCallContent>(
            source: executionNode,
            target: finalizerNode,
            condition: result => result is { Name: "finalize_travel_plan" });

        builder.AddEdge<FunctionCallContent>(
            source: executionNode,
            target: requestInformationNode,
            condition: result => result is { Name: "information_request" });


        builder.AddEdge(requestInformationNode, requestInformationPort);
        builder.AddEdge( requestInformationPort, requestInformationNode);

        builder.AddEdge(requestInformationNode, planningNode);
        builder.AddEdge(travelPlanNode, planningNode);

        return builder.Build();
    }
}