using Application.Agents;
using Application.Infrastructure;
using Application.Observability;
using Application.Workflows.Conversations;
using Microsoft.Extensions.AI;
using Application.Workflows;
using Application.Workflows.Conversations.Dto;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class ApplicationService(IAgentFactory agentFactory, IWorkflowManager workflowManager, IOptions<AzureStorageSeedSettings> settings)
    : IApplicationService
{
    public async Task<ConversationResponse> Execute(ConversationRequest request)
    {
        var initializeActivity = Telemetry.StarActivity("Initialize");

        var reasonAgent = await agentFactory.CreateReasonAgent();

        var actAgent = await agentFactory.CreateActAgent();
      
        initializeActivity?.Dispose();

        var workflowActivity = Telemetry.StarActivity("Workflow");

        workflowActivity?.SetTag("User Input", request.Message);
   
        await workflowManager.Initialize(request.SessionId);

        var workflow = new ConversationWorkflow(reasonAgent, actAgent, workflowManager);

        var response = await workflow.Execute(new ChatMessage(ChatRole.User, request.Message));

        workflowActivity?.Dispose();

        await workflowManager.Save();

        return new ConversationResponse(request.SessionId, response.Message);
    }
}

public interface IApplicationService
{
    Task<ConversationResponse> Execute(ConversationRequest request);
}