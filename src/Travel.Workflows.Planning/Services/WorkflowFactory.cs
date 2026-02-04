using Agents.Services;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Workflows.Planning.Dto;
using Travel.Workflows.Planning.Nodes;

namespace Travel.Workflows.Planning.Services;

public class WorkflowFactory(IAgentFactory agentFactory)
{
    public async Task<Workflow> Build()
    {
        var planningAgent = await agentFactory.Create("planning_agent");

        var planningNode = new PlanningNode(planningAgent);

        var executionNode = new ExecutionNode();
        var travelPlanNode = new TravelPlanNode();
        var requestInformationNode = new RequestInformationNode();

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
            target: requestInformationNode,
            condition: result => result is { Name: "information_request" });


        builder.AddEdge(requestInformationNode, requestInformationPort);

        return builder.Build();
    }
}