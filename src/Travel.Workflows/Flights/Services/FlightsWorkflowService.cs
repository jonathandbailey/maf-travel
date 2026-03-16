using System.Runtime.CompilerServices;
using Microsoft.Agents.AI.Workflows;
using ModelContextProtocol.Client;
using Travel.Agents.Services;
using Travel.Workflows.Flights.Dto;

namespace Travel.Workflows.Flights.Services;

public class FlightsWorkflowService(IAgentProvider agentProvider, McpClient mcpClient)
{
    public async IAsyncEnumerable<WorkflowEvent> SearchAsync(
        FlightsWorkflowRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var flightAgent = await agentProvider.CreateAsync(AgentType.Flight);
        var workflow = FlightsWorkflowBuilder.Build(flightAgent, mcpClient);
        var checkpointManager = CheckpointManager.Default;

        var run = await InProcessExecution.RunStreamingAsync(workflow, request, checkpointManager);

        await foreach (var evt in run.WatchStreamAsync(cancellationToken))
        {
            yield return evt;
        }
    }
}
