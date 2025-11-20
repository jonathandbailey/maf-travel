using Application.Agents;
using Application.Infrastructure;
using Application.Workflows;
using Application.Workflows.ReAct.Dto;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI.Workflows;

namespace Application.Services;

public class ApplicationService(IAgentFactory agentFactory, IWorkflowRepository workflowRepository, ICheckpointRepository repository)
    : IApplicationService
{
    public async Task<ConversationResponse> Execute(ConversationRequest request)
    {
        var reasonAgent = await agentFactory.CreateReasonAgent();

        var actAgent = await agentFactory.CreateActAgent();

        var orchestrationAgent = await agentFactory.CreateOrchestrationAgent();
     
        var state = await workflowRepository.LoadAsync(request.SessionId);

        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore(repository));

        var workflow = new ReActWooWorkflow(reasonAgent, actAgent, orchestrationAgent, checkpointManager, state.CheckpointInfo, state.State);

        var response = await workflow.Execute(new ChatMessage(ChatRole.User, request.Message));
  
        await workflowRepository.SaveAsync(request.SessionId, workflow.State, workflow.CheckpointInfo);

        return new ConversationResponse(request.SessionId, response.Message);
    }
}

public interface IApplicationService
{
    Task<ConversationResponse> Execute(ConversationRequest request);
}