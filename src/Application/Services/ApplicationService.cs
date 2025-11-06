using Application.Agents;
using Application.Workflows.Conversations;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace Application.Services;

public class ApplicationService(IAgentFactory agentFactory) : IApplicationService
{
    public async Task<string> Execute(string userInput)
    {
        var reasonAgent = await agentFactory.CreateReasonAgent();

        var actAgent = await agentFactory.CreateActAgent();

        var checkpointManager = CheckpointManager.CreateJson(new ConversationCheckpointStore());

        var workflow = new ConversationWorkflow(reasonAgent, actAgent, checkpointManager);

        var response = await workflow.Execute(new ChatMessage(ChatRole.User, userInput));

        return response.Message;
    }
}

public interface IApplicationService
{
    Task<string> Execute(string userInput);
}