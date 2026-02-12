using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Workflows.Dto;
using Travel.Workflows.Nodes;

namespace Travel.Workflows.Services;

public class WorkflowFactory(ITravelPlanService travelPlanService)
{
    public Workflow Build(AIAgent planningAgent, AIAgent extractingAgent)
    {
        var planningNode = new PlanningNode(planningAgent);

        var travelPlanNode = new TravelPlanNode(travelPlanService);
      
        var extractingNode = new ExtractingNode(extractingAgent);

        var builder = new WorkflowBuilder(extractingNode);

        var requestInformationPort = RequestPort.Create<InformationRequest, InformationResponse>("information");

        var requestInformationNode = new RequestInformationNode();

        var executionNode = new ExecutionNode();

        builder.AddEdge(extractingNode, travelPlanNode);

        builder.AddEdge(travelPlanNode, planningNode);

        builder.AddEdge(planningNode, executionNode);

        builder.AddEdge<FunctionCallContent>(
            source: executionNode,
            target: requestInformationNode,
            condition: result => result is { Name: "RequestInformation" });

        builder.AddEdge(requestInformationNode, requestInformationPort);

        return builder.Build();
    }
}