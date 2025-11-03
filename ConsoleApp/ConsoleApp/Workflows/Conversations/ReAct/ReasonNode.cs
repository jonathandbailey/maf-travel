using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace ConsoleApp.Workflows.Conversations.ReAct;

public class ReasonNode(AIAgent agent) : ReflectingExecutor<ReasonNode>("ReasonNode"), IMessageHandler<string, string>,
    IMessageHandler<ActObservation,string>
{
    private List<ChatMessage> _messages = [];
   
    public async ValueTask<string> HandleAsync(string message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var requestMessage = new ChatMessage(ChatRole.User, message);

        _messages.Add(requestMessage);

        var response = await agent.RunAsync(_messages, cancellationToken: cancellationToken);

        _messages.Add(new ChatMessage(ChatRole.Assistant, response.Text));
    
        return response.Text;
    }

    protected override ValueTask OnCheckpointingAsync(IWorkflowContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return context.QueueStateUpdateAsync("ChatHistory-Reason", _messages, cancellationToken);
    }

    protected override async ValueTask OnCheckpointRestoredAsync(IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _messages = await context.ReadStateAsync<List<ChatMessage>>("ChatHistory-Reason");
    }

    public async ValueTask<string> HandleAsync(ActObservation actObservation, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var requestMessage = new ChatMessage(ChatRole.User, actObservation.Message);

        _messages.Add(requestMessage);

        var response = await agent.RunAsync(_messages, cancellationToken: cancellationToken);

        _messages.Add(new ChatMessage(ChatRole.Assistant, response.Text));

        return response.Text;
    }
}