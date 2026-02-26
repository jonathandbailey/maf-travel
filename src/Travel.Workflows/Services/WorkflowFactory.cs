using Infrastructure.Repository;
using Travel.Agents.Services;
using Travel.Workflows.Infrastructure;
using Travel.Workflows.Interfaces;

namespace Travel.Workflows.Services;

public class WorkflowFactory(
    ICheckpointRepository checkpointRepository,
    IWorkflowSessionRepository sessionRepository,
    IAgentProvider agentProvider) : IWorkflowFactory
{
    public Task<TravelWorkflowService> Create()
    {
        var workflowService = new TravelWorkflowService(
            checkpointRepository,
            sessionRepository,
            agentProvider);

        return Task.FromResult(workflowService);
    }
}
