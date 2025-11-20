using Application.Agents;
using Application.Observability;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace Application.Workflows.ReWoo.Nodes;

public class OrchestrationNode(IAgent agent) : ReflectingExecutor<OrchestrationNode>("OrchestrationNode"), IMessageHandler<OrchestrationRequest>
{
    public async ValueTask HandleAsync(OrchestrationRequest message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start("OrchestrationActHandleRequest");

        activity?.SetTag("re-woo.node", "orchestration_node");

        activity?.SetTag("re-woo.input.message", message.Text);
    }
}