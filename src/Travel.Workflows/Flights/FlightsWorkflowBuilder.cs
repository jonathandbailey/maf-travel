using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using ModelContextProtocol.Client;
using Travel.Workflows.Flights.Nodes;

namespace Travel.Workflows.Flights;

public class FlightsWorkflowBuilder
{
    public static Workflow Build(AIAgent flightAgent, McpClient mcpClient)
    {
        var startNode = new StartNode();
        var flightNode = new FlightNode(flightAgent);
        var searchNode = new SearchNode(mcpClient);
        var endNode = new FlightsEndNode();

        var builder = new WorkflowBuilder(startNode);

        builder.AddEdge(startNode, flightNode);
        builder.AddEdge(flightNode, searchNode);
        builder.AddEdge(searchNode, endNode);

        return builder.Build();
    }
}
