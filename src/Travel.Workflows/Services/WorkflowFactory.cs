using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Agents.Services;
using Travel.Workflows.Dto;
using Travel.Workflows.Nodes;

namespace Travel.Workflows.Services;

public class WorkflowFactory
{
    public Workflow Build(AIAgent planningAgent, AIAgent extractingAgent)
    {
        var planningNode = new PlanningNode(planningAgent);

        var travelPlanNode = new TravelUpdatePlanNode();

        var startNode = new StartNode();

        var extractingNode = new ExtractingNode(extractingAgent);

        var builder = new WorkflowBuilder(startNode);

        var requestInformationPort = RequestPort.Create<InformationRequest, InformationResponse>("information");

        var requestInformationNode = new RequestInformationNode();

        var executionNode = new ExecutionNode();

        var endNode = new EndNode();

        builder.AddEdge(startNode, extractingNode);

        builder.AddEdge(extractingNode, travelPlanNode);

        builder.AddEdge(travelPlanNode, planningNode);

        builder.AddEdge(planningNode, executionNode);

        builder.AddEdge(executionNode, endNode);

        builder.AddEdge<FunctionCallContent>(
            source: executionNode,
            target: requestInformationNode,
            condition: result => result is { Name: PlanningTools.RequestInformationToolName });

      
            

        builder.AddEdge(requestInformationNode, requestInformationPort);
        builder.AddEdge(requestInformationPort, requestInformationNode);

        builder.AddEdge(requestInformationNode, extractingNode);

        var workflow = builder.Build();

    

        return workflow;
    }
}