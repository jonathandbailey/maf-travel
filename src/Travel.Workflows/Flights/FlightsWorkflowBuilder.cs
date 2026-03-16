using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Nodes;

namespace Travel.Workflows.Flights;

public class FlightsWorkflowBuilder
{
    public static Workflow Build()
    {
        var startNode = new StartNode();

        var builder = new WorkflowBuilder(startNode);

        var workflow = builder.Build();

        return workflow;
    }
}