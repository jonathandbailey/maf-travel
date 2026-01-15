using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Workflows.Dto;

namespace Workflows.Nodes;

public class StartNode() : ReflectingExecutor<StartNode>("StartNode") ,
IMessageHandler<StartWorkflowDto, ReasoningInputDto>
{
    public async ValueTask<ReasoningInputDto> HandleAsync(StartWorkflowDto message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return message.ReasoningInputDto;
    }
}