using A2A;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Travel.Workflows.Dto;

namespace Travel.Workflows.Nodes;

public class StartNode() : ReflectingExecutor<StartNode>("StartNode") ,
IMessageHandler<StartWorkflowDto, ReasoningInputDto>
{
    public async ValueTask<ReasoningInputDto> HandleAsync(StartWorkflowDto message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        await context.QueueStateUpdateAsync("agent_thread_id", message.ThreadId, scopeName:"workflow", cancellationToken: cancellationToken);

        return message.ReasoningInputDto;
    }
}