using Agents.Services;
using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Planning.Nodes;

namespace Travel.Workflows.Planning.Services;

public class WorkflowFactory(IAgentFactory agentFactory)
{
    public async Task<Workflow> Build()
    {
        var planningAgent = await agentFactory.Create("planning_agent");

        var planning = new PlanningNode(planningAgent);

        var builder = new WorkflowBuilder(planning);

        return builder.Build();
    }
}